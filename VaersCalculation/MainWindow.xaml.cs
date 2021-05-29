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

        private void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLineCalculationDiff(FileSourceTextBox.Text);
        }

        public async void PerformLineCalculationDiff(string filePath)
        {
            var ids = await ReadFileContents(filePath);
            var totalSkippedLines = 0;
            var previousLineId = ids.First();
            var outputText = "";
            await Task.Factory.StartNew(() =>
            {
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
            });
            TotalSkippedIdsTextBox.Text = totalSkippedLines.ToString();
            ContentsTextBox.Text = outputText;
        }

        public Task<List<int>> ReadFileContents(string filePath)
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
            AllFileLines.Sort();
            return Task.FromResult(AllVAERSIdNumbers);
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
