using ExcelDataReader;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;

namespace MYKCalculator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dataPath = "";

        private DataSet loadedDataSet;
        private int selectedTableIndex;

        public MainWindow()
        {
            InitializeComponent();

            cmbbox_TableChooser.SelectionChanged += Cmbbox_TableChooser_SelectionChanged;
        }

        private void Cmbbox_TableChooser_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedTableIndex = cmbbox_TableChooser.SelectedIndex;
            LoadSelectedDataTable(loadedDataSet.Tables[selectedTableIndex].Copy());
        }

        private void btn_xlsPickerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                dataPath = lbl_Filepath.Text = openFileDialog.FileName;
                btn_ImportData.IsEnabled = true;
            }
        }



        private void btn_ImportData_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData));
            importThread.Start();
        }

        private void ImportXLSData()
        {
            using (var stream = File.Open(dataPath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    loadedDataSet = reader.AsDataSet();

                    // Combobox zur Tabellenwahl füllen
                    List<string> tableNames = new List<string>();
                    foreach (DataTable dataTable in loadedDataSet.Tables)
                    {
                        tableNames.Add(dataTable.TableName);
                    }

                    Dispatcher.Invoke((() => FillCombobox(tableNames))); ;
                }
            }
        }

        private void FillCombobox(List<string> tableNames)
        {
            cmbbox_TableChooser.ItemsSource = tableNames;
            cmbbox_TableChooser.IsEnabled = true;
            cmbbox_TableChooser.SelectedIndex = 0;
        }

        private void LoadSelectedDataTable(DataTable loadedTable)
        {
            dgrid_DataGrid.DataContext = loadedTable;
            SetColumnNames(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            btn_CalculateScore.IsEnabled = true;
        }

        private void SetColumnNames(DataTable data)
        {
            for (int n = 0; n < dgrid_DataGrid.Columns.Count; n++)
            {
                dgrid_DataGrid.Columns[n].Header = data.Rows[0].ItemArray[n];
            }
        }

        private void CalculateScore()
        {
            int currentScore = 0;
            int currentRowIndex = 1;

            for (; currentRowIndex < loadedDataSet.Tables[selectedTableIndex].Rows.Count; currentRowIndex++)
            {
                currentScore = 0;
                DataRow currentRow = loadedDataSet.Tables[selectedTableIndex].Rows[currentRowIndex];

                // single
                int singleColumnIndex = FindIndexToColumn("single");
                if (currentRow[singleColumnIndex].ToString() == "1")
                {
                    currentScore++;
                }

                // from Pos
                int fromPosIndex = FindIndexToColumn("from_pos");
                float outFromPos;
                if (float.TryParse(currentRow[fromPosIndex].ToString(), out outFromPos))
                {
                    if (outFromPos < 127640739.7 || outFromPos > 130113749.6)
                        currentScore++;
                }

                // Vaf_Mean
                int vafMeanIndex = FindIndexToColumn("Vaf_Mean");
                float outVafMean;
                if (float.TryParse(currentRow[vafMeanIndex].ToString(), out outVafMean))
                {
                    if (outVafMean <= 0.03)
                        currentScore++;
                }
                else
                {
                    int vafDiffIndex = FindIndexToColumn("Vaf_Differenz");
                    float outVafDiff;
                    if (float.TryParse(currentRow[vafDiffIndex].ToString(), out outVafDiff))
                    {
                        if (outVafDiff >= 0.136350466)
                        {
                            int vafSR_Index = FindIndexToColumn("vaf_SR");
                            int vafPR_Index = FindIndexToColumn("vaf_PR");
                            float outVafSR;
                            float outVafPR;
                            if (float.TryParse(currentRow[vafSR_Index].ToString(), out outVafSR))
                            {
                                if (outVafSR <= 0.03)
                                    currentScore++;
                            }
                            else if (float.TryParse(currentRow[vafPR_Index].ToString(), out outVafPR))
                            {
                                if (outVafPR <= 0.03)
                                    currentScore++;
                            }
                        }
                    }
                }

                // Reads_Mean
                int readsMeanIndex = FindIndexToColumn("Reads_Mean");
                float outReadsMean;
                if (float.TryParse(currentRow[readsMeanIndex].ToString(), out outReadsMean))
                {
                    if (outReadsMean <= 5)
                        currentScore++;
                }
                else
                {
                    int readsDiffIndex = FindIndexToColumn("Reads_Differenz");
                    float outReadsDif;
                    if (float.TryParse(currentRow[readsDiffIndex].ToString(), out outReadsDif))
                    {
                        if (outReadsDif >= 24.55288628)
                        {
                            int readsSR_Index = FindIndexToColumn("tumor_alt_SR");
                            int readsPR_Index = FindIndexToColumn("tumor_alt_PR");
                            float outReadsSR;
                            float outReadsPR;
                            if (float.TryParse(currentRow[readsSR_Index].ToString(), out outReadsSR))
                            {
                                if (outReadsSR <= 5)
                                    currentScore++;
                            }
                            else if (float.TryParse(currentRow[readsPR_Index].ToString(), out outReadsPR))
                            {
                                if (outReadsPR <= 5)
                                    currentScore++;
                            }
                        }
                    }
                }


                // MYC Expression
                int mycExpressionIndex = FindIndexToColumn("MYC Expression");
                float outMycExp;
                if (float.TryParse(currentRow[mycExpressionIndex].ToString(), out outMycExp))
                {
                    if (outMycExp <= 4.46)
                        currentScore++;
                }

                // n_myl
                int nMylIndex = FindIndexToColumn("n_myl");
                float outNMyl;
                if (float.TryParse(currentRow[nMylIndex].ToString(), out outNMyl))
                {
                    if (outNMyl == 2)
                    {
                        currentScore++;
                    }
                    else if (outNMyl > 2)
                    {
                        currentScore++;
                        currentScore++;
                    }
                }



                SetScore(currentScore, currentRowIndex);
            }

            // Refresh data
            LoadSelectedDataTable(loadedDataSet.Tables[selectedTableIndex].Copy());
        }

        private void SetScore(int scoreToSet, int rowIndex)
        {
            int scoreColumnIndex = FindIndexToColumn("Score");
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].ItemArray[scoreColumnIndex] = scoreToSet;
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].SetField(scoreColumnIndex, scoreToSet);
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].AcceptChanges();
        }

        private int FindIndexToColumn(string searchKeyword)
        {
            List<string> headerList = new List<string>();
            foreach (object obj in loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray)
            {
                headerList.Add(obj.ToString());
            }

            return headerList.FindIndex(x => x.Equals(searchKeyword));
        }

        private void btn_CalculateScore_Click(object sender, RoutedEventArgs e)
        {
            CalculateScore();
           
        }

        private void ExportDataSetToExcel(DataSet ds)
        {
            dgrid_DataGrid.SelectAll();
            //DataObject dataObj = dgrid_DataGrid.GetClipboardContent();
           // if (dataObj != null)
             //   Clipboard.SetDataObject(dataObj);
        }

        private void btn_ExportData_Click(object sender, RoutedEventArgs e)
        {
            ExportDataSetToExcel(loadedDataSet);
        }
    }
}
