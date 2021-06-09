using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VaersCalculation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UIViewModel reportingViewModel = new UIViewModel
        {
            TotalLinesSkipped = 0,
            AggregateReportOut = "Report will appear here (if checked)",
            FileToReadText = "C:/data/2021VAERSData.csv",
            OutputReportFolderPath = "C:/data/"
        };

        private static CheckBox _forceWindowTopMostCheckBox; 

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = reportingViewModel;
            
        }

        public void SetTopMostWindowFromCheck(bool topmost)
        {
            
        }

        public class VaersRecord
        {
            public string PreviousLineDate { get; set; }
            public string LineDate { get; set; }
            public int Id { get; set; }
            public int TotalLinesSkipped { get; set; }
            public int SkippedLinesAtThisId { get; set; }
            public int LineId { get; set; }
        }

        public static int scrollContentsLineChangedCount = 0;

        public void SetContentsText(string text)
        {
            reportingViewModel.FileContentsOutput = text + reportingViewModel.FileContentsOutput;
            //if (scrollContentsLineChangedCount > 10)
            //{

            //    //Application.Current.Dispatcher.Invoke(() => { this.ContentsTextBox.ScrollToEnd(); });
            //    scrollContentsLineChangedCount = 0;
            //    return;
            //}
            //scrollContentsLineChangedCount++;
        }

        public class UIViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            private int _totalLinesSkipped = 0;
            public int TotalLinesSkipped
            {
                get { return _totalLinesSkipped; }
                set
                {
                    _totalLinesSkipped = value;
                    this.OnPropertyChanged();
                }
            }
            private string _aggregateReportOut = "";
            public string AggregateReportOut
            {
                get { return _aggregateReportOut; }
                set
                {
                    _aggregateReportOut = value;
                    this.OnPropertyChanged();
                }
            }
            private string _fileContentsOutput { get; set; }
            public string FileContentsOutput
            {
                get { return _fileContentsOutput; }
                set
                {
                    _fileContentsOutput = value;
                    this.OnPropertyChanged();

                }
            }
            private string _fileToReadText = "";
            public string FileToReadText
            {
                get { return _fileToReadText; }
                set
                {
                    _fileToReadText = value;
                    this.OnPropertyChanged();
                    try
                    {
                        _fileModifiedWhen = System.IO.File.GetLastWriteTime(value).ToString().Split('\\').Last().Replace('/', '_').Replace(' ', '_').Replace(':', '_');
                    }
                    catch
                    {
                        
                    }
                }
            }
            private string _fileToReadStatus = "";
            public string FileReadStatus
            {
                get { return _fileToReadStatus; }
                set
                {
                    _fileToReadStatus = value;
                    this.OnPropertyChanged();
                }
            }
            private int _fileStartLineId = 0;
            public int FileStartLineId
            {
                get { return _fileStartLineId; }
                set
                {
                    _fileStartLineId = value;
                    this.OnPropertyChanged();
                }
            }
            private int _fileEndLineId = 0;
            public int FileEndLineId
            {
                get { return _fileEndLineId; }
                set
                {
                    _fileEndLineId = value;
                    this.OnPropertyChanged();
                }
            }
            private int _fileLinesToBeProcessed = 0;
            public int FileLinesToBeProcessed
            {
                get { return _fileLinesToBeProcessed; }
                set
                {
                    _fileLinesToBeProcessed = value;
                    this.OnPropertyChanged();
                }
            }
            private string _fileSkippedLinePercentage = "0";
            public string FileSkippedLinePercentage
            {
                get { return _fileSkippedLinePercentage; }
                set
                {
                    _fileSkippedLinePercentage = value;
                    this.OnPropertyChanged();
                }
            }
            private string _percentFinishedProcessing = "0";
            public string PercentFinishedProcessing
            {
                get { return _percentFinishedProcessing; }
                set
                {
                    _percentFinishedProcessing = value;
                    this.OnPropertyChanged();
                }
            }
            private string _outputReportFolderPath = "";
            public string OutputReportFolderPath
            {
                get { return _outputReportFolderPath; }
                set
                {
                    _outputReportFolderPath = value;
                    this.OnPropertyChanged();
                }
            }
            private string _fileModifiedWhen = "";
            public string FileModifiedWhen
            {
                get { return _fileModifiedWhen; }
                set
                {
                    _fileModifiedWhen = value;
                    this.OnPropertyChanged();
                }
            }
            private bool _windowForceTopMost = false;
            public bool WindowForceTopMost
            {
                get { return _windowForceTopMost; }
                set
                {
                    _windowForceTopMost = value;
                    this.OnPropertyChanged();
                   
                }
            }
        }

        public string GetFinishedLinePercentage(int currentLine, double totalLines)
        {
            try
            {
                return ($"{((double)currentLine / (double)totalLines) * 100}").Substring(0, 6);
            }
            catch
            {
                return "100"; // only happens when double hits 100, need to fix this
            }
        }

        public string GetSkippedLinePercentage(int skippedLines, int lineNumber)
        {
            try
            {
                return ($"{((double)skippedLines / (double)lineNumber) * 100}").Substring(0, 6);
            }
            catch
            {
                return "N/A";
            }
        }

        private void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLineCalculationDiff(FileSourceTextBox.Text, (bool)GenerateFullIdReportCheckBox.IsChecked);
        }


        public async void PerformLineCalculationDiff(string filePath, bool generateReport = false)
        {
            var ids = new List<int>();
            var vaersLines = new List<string>();
            reportingViewModel.TotalLinesSkipped = 0;
            reportingViewModel.FileContentsOutput = "performing line calculation diff";
            reportingViewModel.AggregateReportOut = "initializing report...";
            try
            {
                if (!generateReport)
                {
                    ids = await ReadFileContentsAsInts(filePath);
                }
                else
                {
                    vaersLines = await ReadFileContentsAsStrings(filePath);
                }
            }
            catch (Exception ex)
            {
                reportingViewModel.FileReadStatus = ex.Message;
                return;
            }

            await Task.Factory.StartNew(() =>
            {
                if (!generateReport)
                {
                    var previousLineId = ids.First();
                    foreach (var lineId in ids)
                    {
                        var lineDiff = lineId - previousLineId;
                        if (lineDiff > 1)
                        {
                            reportingViewModel.TotalLinesSkipped += lineDiff - 1;
                            SetContentsText($"<<<<<<<<<<<< {lineDiff - 1} Skipped Lines detected of total {reportingViewModel.TotalLinesSkipped}!!!!>>>>>>>>>" + "\n");
                        }
                        reportingViewModel.FileContentsOutput = lineId.ToString() + "\n" + reportingViewModel.FileContentsOutput;
                        previousLineId = lineId;
                    }
                }
                else
                {
                    var previousLineId = 0;
                    var previousLineDate = "";
                    var recordId = 0;
                    var lineDate = "";
                    var missingDatePairsById = new List<VaersRecord>();
                    var lineNumber = 0;
                    var startDate = "";
                    var endDate = "";
                    double linesToBeProcessedDouble = 0;
                    try
                    {
                        startDate = vaersLines.First().Split(',')[1];
                        endDate = vaersLines.Last().Split(',')[1];
                        previousLineId = Convert.ToInt32(vaersLines.First().Split(',')[0]);
                        recordId = Convert.ToInt32(vaersLines.First().Split(',')[0]);
                        reportingViewModel.FileStartLineId = previousLineId;
                        reportingViewModel.FileEndLineId = Convert.ToInt32(vaersLines.Last().Split(',')[0]);
                        reportingViewModel.FileLinesToBeProcessed = reportingViewModel.FileEndLineId - reportingViewModel.FileStartLineId;
                        linesToBeProcessedDouble = Convert.ToDouble(reportingViewModel.FileLinesToBeProcessed);
                    }
                    catch (Exception ex)
                    {
                        reportingViewModel.FileReadStatus = $"File input was not recognized; check your csv. Error: {ex.Message}";
                        return;
                    }
                    foreach (var vaersLine in vaersLines)
                    {
                        recordId = Convert.ToInt32(vaersLine.Split(',')[0]);
                        var lineDiff = Convert.ToInt32(recordId) - Convert.ToInt32(previousLineId);
                        lineDate = vaersLine.Split(',')[1];
                        if (lineDiff > 1)
                        {
                            reportingViewModel.TotalLinesSkipped += lineDiff - 1;
                            SetContentsText($"<<<<<<<<<< {lineDiff - 1} Skipped, total {reportingViewModel.TotalLinesSkipped}, range {previousLineDate}=>{lineDate}>>>>>>>>>" + "\n");
                            missingDatePairsById.Add(new VaersRecord
                            {
                                Id = recordId,
                                PreviousLineDate = previousLineDate,
                                LineDate = lineDate,
                                TotalLinesSkipped = reportingViewModel.TotalLinesSkipped,
                                SkippedLinesAtThisId = lineDiff - 1,
                                LineId = lineNumber
                            });
                        }
                        lineNumber++;
                        reportingViewModel.PercentFinishedProcessing = GetFinishedLinePercentage(lineNumber + reportingViewModel.TotalLinesSkipped, linesToBeProcessedDouble);
                        reportingViewModel.FileSkippedLinePercentage = GetSkippedLinePercentage(reportingViewModel.TotalLinesSkipped, lineNumber);
                        SetContentsText(recordId.ToString() + " | " + lineDate + $" | line #: {lineNumber}" + "\n");
                        previousLineDate = lineDate;
                        previousLineId = recordId;
                    }
                    reportingViewModel.AggregateReportOut = GenerateOutputReport(missingDatePairsById, reportingViewModel.TotalLinesSkipped, lineNumber, linesToBeProcessedDouble, reportingViewModel.FileStartLineId, reportingViewModel.FileEndLineId, startDate, endDate);
                }
            });
        }

        public static string GenerateOutputReport(List<VaersRecord> records, int totalSkippedLines, int totalLines, double realLineTotal, int startLineId, int endLineId, string startDate, string endDate)
        {
            string outputReport = "";

            outputReport = $"Summary: " + "\n" + $"A total of {totalSkippedLines} skipped IDs detected." + "\n" +
                $"Id/Date range = {startLineId} ({startDate}) => {endLineId} ({endDate})" + "\n" +
                $"which makes for a total of {realLineTotal} IDs in range. " + "\n" +
                $"the total percentage of skipped Ids relative to lines is: " +
                $"{((double)totalSkippedLines / (double)totalLines) * 100}%" + "\n" +
                $"==========begin detail============" + "\n";

            foreach (var record in records)
            {
                outputReport += $"{record.SkippedLinesAtThisId} Missing IDs@ {record.Id} from {record.PreviousLineDate} => {record.LineDate} of total {record.TotalLinesSkipped}" + "\n";
            }
            return outputReport;
        }

        public Task<List<int>> ReadFileContentsAsInts(string filePath)
        {
            StatusTextBox.Text = $"reading file path {filePath}";
            var allFileLines = new List<string>();
            allFileLines = System.IO.File.ReadAllLines(filePath).ToList();
            var allVAERSIdNumbers = new List<int>();
            ContentsTextBox.Text = "";
            for (int i = 1; i < allFileLines.Count; i++)
            {
                var lineIdInt = Convert.ToInt32(allFileLines[i].Split(',')[0]);
                allVAERSIdNumbers.Add(lineIdInt);
            }
            allVAERSIdNumbers.Sort();
            return Task.FromResult(allVAERSIdNumbers);
        }

        public Task<List<string>> ReadFileContentsAsStrings(string filePath)
        {
            reportingViewModel.FileReadStatus = $"reading file path {filePath}";
            var AllFileLines = new List<string>();
            try
            {
                AllFileLines = System.IO.File.ReadAllLines(filePath).ToList();

            }
            catch (Exception ex)
            {
                reportingViewModel.FileReadStatus = ex.Message;
                return Task.FromResult(AllFileLines);
            }
            AllFileLines.RemoveAt(0);
            reportingViewModel.FileContentsOutput = "";
            var ordered = AllFileLines.Select(s => new { Str = s, Split = s.Split(',') })
                .OrderBy(x => int.Parse(x.Split[0]))
                //.ThenBy(x => x.Split[1])
                .Select(x => x.Str)
                .ToList();
            return Task.FromResult(ordered);
        }

        private void VAERSCsvFileChooser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog file = new OpenFileDialog();
                file.ShowDialog();
                if (file.FileNames != null)
                {
                    reportingViewModel.FileToReadText = file.FileNames[0];
                }

            }
            catch
            {

            }

        }

        public static string SelectedFileDateTimeCreated = "";

        private void SelectAnOutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    reportingViewModel.OutputReportFolderPath = $"{dialog.FileName}\\";
                }
            }
            catch (Exception ex)
            {
                reportingViewModel.FileReadStatus = ex.Message;
            }
        }

        private void WriteReportsToFilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteReportsToFiles(reportingViewModel.FileContentsOutput, reportingViewModel.AggregateReportOut, reportingViewModel.OutputReportFolderPath);
            }
            catch (Exception ex)
            {
                reportingViewModel.FileReadStatus = ex.Message;
            }

        }

        public void WriteReportsToFiles(string fileContentsOutput, string aggregateReportOut, string folderPath, bool combineReports = false)
        {
            if (!combineReports)
            {
                if (fileContentsOutput != null)
                {
                    System.IO.File.WriteAllText($"{folderPath}{GenerateFileName("FileContentsOutput", reportingViewModel.FileModifiedWhen)}", fileContentsOutput);
                }
                if (aggregateReportOut != null)
                {
                    System.IO.File.WriteAllText($"{folderPath}{GenerateFileName("AggregateVAERSReport", reportingViewModel.FileModifiedWhen)}", aggregateReportOut);

                }
            }
        }

        public string GenerateFileName(string desiredName, string fileModifiedWhen)
        {
            return $"{desiredName}_{fileModifiedWhen}_{DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_')}_analyzed_by_hexagod_VAERSCalculation.txt";
        }

        private void ForceWindowTopMostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = Convert.ToBoolean(reportingViewModel.WindowForceTopMost);
        }

        private void ForceWindowTopMostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = Convert.ToBoolean(reportingViewModel.WindowForceTopMost);

        }
    }
}
