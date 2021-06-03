using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VaersCalculation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<string> AllFileLines = new List<string>();

        public class VaersRecord
        {
            public string PreviousLineDate { get; set; }
            public string LineDate { get; set; }
            public int Id { get; set; }
            public int TotalSkippedLines { get; set; }
            public int SkippedLinesAtThisId { get; set; }
            public int LineId { get; set; }
        }

        private void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUiValues();
            PerformLineCalculationDiff(FileSourceTextBox.Text, (bool)GenerateFullIdReportCheckBox.IsChecked);
        }
        
        public async void PerformLineCalculationDiff(string filePath, bool generateReport = false)
        {
            var ids = new List<int>();
            var vaersLines = new List<string>();
            var totalSkippedLines = 0;
            var outputText = "";
            var outputReport = "";
            if (!generateReport)
            {
                ids = await ReadFileContentsAsInts(filePath);
            }
            else
            {
                vaersLines = await ReadFileContentsAsStrings(filePath);
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
                            totalSkippedLines += lineDiff - 1;
                            outputText += $"<<<<<<<<<<<< {lineDiff - 1} Skipped Lines detected of total {totalSkippedLines}!!!!>>>>>>>>>" + "\n";
                        }
                        outputText += lineId.ToString() + "\n";
                        previousLineId = lineId;
                    }
                }
                else
                {
                    var previousLineId = Convert.ToInt32(vaersLines.First().Split(',')[0]);
                    var previousLineDate = "";
                    var recordId = Convert.ToInt32(vaersLines.First().Split(',')[0]);
                    var lineDate = "";
                    //var missingDatePairsById = new List<Tuple<int, string, string, int, int>>();
                    var missingDatePairsById = new List<VaersRecord>();
                    var lineNumber = 0;
                    foreach (var vaersLine in vaersLines)
                    {
                        recordId = Convert.ToInt32(vaersLine.Split(',')[0]);
                        var lineDiff = Convert.ToInt32(recordId) - Convert.ToInt32(previousLineId);
                        lineDate = vaersLine.Split(',')[1];
                        if (lineDiff > 1)
                        {
                            totalSkippedLines += lineDiff - 1;
                            outputText += $"<<<<<<<<<< {lineDiff - 1} Skipped, total {totalSkippedLines}, range {previousLineDate}=>{lineDate}>>>>>>>>>" + "\n";
                            missingDatePairsById.Add(new VaersRecord
                            {
                                Id = recordId,
                                PreviousLineDate = previousLineDate,
                                LineDate = lineDate,
                                TotalSkippedLines = totalSkippedLines,
                                SkippedLinesAtThisId = lineDiff - 1,
                                LineId = lineNumber
                            });
                            //missingDatePairsById.Add(new Tuple<int, string, string, int, int>(lineId, previousLineDate, lineDate, totalSkippedLines,(lineDiff-1)));
                        }
                        lineNumber++;
                        outputText += recordId.ToString() + " | " + lineDate + $" | line #: {lineNumber}" + "\n";
                        previousLineDate = lineDate;
                        previousLineId = recordId;
                    }
                    outputReport = GenerateOutputReport(missingDatePairsById, totalSkippedLines, lineNumber);
                }
            });
            OutputResultsToUi(totalSkippedLines, outputText, outputReport);
        }

        public static string GenerateOutputReport(List<VaersRecord> records, int totalSkippedLines, int totalLines)
        {
            string outputReport = "";
            var startDate = records[0].PreviousLineDate;
            var endDate = records.Last().LineDate;
            var startId = records[0].Id;
            var endId = records.Last().Id;
            outputReport = $"Summary: " + "\n" + $"A total of {totalSkippedLines} skipped IDs detected." + "\n" +
                $"Id/Date range = {startId} ({startDate}) => {endId} ({endDate})" + "\n" +
                $"which makes for a total of {endId - startId} IDs in range. " + "\n" +
                $"the total percentage of skipped Ids relative to lines is: " +
                $"{((double)totalSkippedLines/(double)totalLines) * 100}%" + "\n" + 
                $"==========begin detail============" + "\n";
            
            foreach (var record in records)
            {
                outputReport += $"{record.SkippedLinesAtThisId} Missing IDs@ {record.Id} from {record.PreviousLineDate} => {record.LineDate} of total {record.TotalSkippedLines}" + "\n";
            }
            return outputReport;
        }

        public void ClearUiValues()
        {
            OutputResultsToUi(0, " ", " ");
        }

        public void OutputResultsToUi(int totalSkippedLines, string aggregateTextOutput, string outputReport = "")
        {
            TotalSkippedIdsTextBox.Text = totalSkippedLines.ToString();
            ContentsTextBox.Text = aggregateTextOutput;
            if (outputReport != "")
            {
                ReportTextBox.Text = outputReport;
            }
        }

        public Task<List<int>> ReadFileContentsAsInts(string filePath)
        {
            StatusTextBox.Text = $"reading file path {filePath}";
            if (AllFileLines.Count > 0)
            {
                AllFileLines = new List<string>();
            }
            AllFileLines = System.IO.File.ReadAllLines(filePath).ToList();
            var AllVAERSIdNumbers = new List<int>();
            ContentsTextBox.Text = "";
            for (int i = 1; i < AllFileLines.Count; i++)
            {
                var lineIdInt = Convert.ToInt32(AllFileLines[i].Split(',')[0]);
                AllVAERSIdNumbers.Add(lineIdInt);
            }
            AllVAERSIdNumbers.Sort();
            return Task.FromResult(AllVAERSIdNumbers);
        }

        public Task<List<string>> ReadFileContentsAsStrings(string filePath)
        {
            StatusTextBox.Text = $"reading file path {filePath}";
            if (AllFileLines.Count > 0)
            {
                AllFileLines = new List<string>();
            }
            AllFileLines = System.IO.File.ReadAllLines(filePath).ToList();
            AllFileLines.RemoveAt(0);
            ContentsTextBox.Text = "";
            var ordered = AllFileLines.Select(s => new { Str = s, Split = s.Split(',') })
                .OrderBy(x => int.Parse(x.Split[0]))
                //.ThenBy(x => x.Split[1])
                .Select(x => x.Str)
                .ToList();
            return Task.FromResult(ordered);
        }

        private void VAERSCsvFileChooser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            if (file.FileNames != null)
            {
                FileSourceTextBox.Text = file.FileName;
            }
        }
    }
}
