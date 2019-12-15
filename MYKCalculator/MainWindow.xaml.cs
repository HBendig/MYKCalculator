using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MYCCalculator
{
    class Patient
    { 
        public string ArrayID;
        public string Entitaet;
        public Dictionary<string, int> GenListe = new Dictionary<string, int>();
       
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dataPath = "";
        private string dataPath_mutations = "";

        private DataSet loadedDataSet;
        private DataSet loadedDataSet_mutations;
        private int selectedTableIndex;
        private int selectedTableIndex_mutations;

        List<string> csvLines = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            cmbbox_TableChooser.SelectionChanged += Cmbbox_TableChooser_SelectionChanged;
            cmbbox_TableChooser2.SelectionChanged += Cmbbox_TableChooser_mutations_SelectionChanged;

            LoadingGrid.IsEnabled = false;
            LoadingGrid.Visibility = Visibility.Hidden;
        }

        private void Cmbbox_TableChooser_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedTableIndex = cmbbox_TableChooser.SelectedIndex;
            LoadSelectedDataTable(loadedDataSet.Tables[selectedTableIndex].Copy());
        }
        private void Cmbbox_TableChooser_mutations_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedTableIndex_mutations = cmbbox_TableChooser2.SelectedIndex;
            LoadSelectedDataTable_mutations(loadedDataSet_mutations.Tables[selectedTableIndex_mutations].Copy());
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

        private void btn_xlsPickerButton2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                dataPath_mutations = lbl_Filepath2.Text = openFileDialog.FileName;
                btn_ImportData2.IsEnabled = true;
            }
        }

        private void btn_ImportData2_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData_Mutations));
            importThread.Start();
        }

        private void ImportXLSData_Mutations()
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            using (var stream = File.Open(dataPath_mutations, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    loadedDataSet_mutations = reader.AsDataSet();
                }
            }

            // Combobox zur Tabellenwahl füllen
            List<string> tableNames = new List<string>();
            foreach (DataTable dataTable in loadedDataSet_mutations.Tables)
            {
                tableNames.Add(dataTable.TableName);
            }

            Dispatcher.Invoke((() => FillCombobox2(tableNames))); ;
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }

        private void btn_ImportData_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData));
            importThread.Start();
        }

        private void ImportXLSData()
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

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
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }

        private void ShowLoadingScreen(bool _isVisible)
        {
            if(_isVisible)
            {
                LoadingGrid.IsEnabled = true;
                LoadingGrid.Visibility = Visibility.Visible;
            } else
            {
                LoadingGrid.IsEnabled = false;
                LoadingGrid.Visibility = Visibility.Hidden;
            }
        }

        private void FillCombobox(List<string> tableNames)
        {
            cmbbox_TableChooser.ItemsSource = tableNames;
            cmbbox_TableChooser.IsEnabled = true;
            cmbbox_TableChooser.SelectedIndex = 0;
        }
        
        private void FillCombobox2(List<string> tableNames)
        {
            cmbbox_TableChooser2.ItemsSource = tableNames;
            cmbbox_TableChooser2.IsEnabled = true;
            cmbbox_TableChooser2.SelectedIndex = 0;
        }

        private void LoadSelectedDataTable(DataTable loadedTable)
        {
            dgrid_DataGrid.DataContext = loadedTable;
            SetColumnNames(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            btn_CalculateScore.IsEnabled = true;
        }
        private void LoadSelectedDataTable_mutations(DataTable loadedTable)
        {
            dgrid_DataGrid2.DataContext = loadedTable;
            SetColumnNames_mutations(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            btn_Analyse.IsEnabled = true;
        }

        private void SetColumnNames(DataTable data)
        {
            for (int n = 0; n < dgrid_DataGrid.Columns.Count; n++)
            {
                dgrid_DataGrid.Columns[n].Header = data.Rows[0].ItemArray[n];
            }
        }
        private void SetColumnNames_mutations(DataTable data)
        {
            for (int n = 0; n < dgrid_DataGrid2.Columns.Count; n++)
            {
                dgrid_DataGrid2.Columns[n].Header = data.Rows[0].ItemArray[n];
            }
        }

        private void CalculateScore()
        {
            float currentScore = 0;
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
                    currentScore++;
                }

                // from Pos
                int fromPosIndex = FindIndexToColumn("from_pos");
                float outFromPos;
                if (float.TryParse(currentRow[fromPosIndex].ToString(), out outFromPos))
                {
                    if (outFromPos < 128698588f || outFromPos > 129113499f) 
                    { 
                        currentScore++;
                        if (outFromPos < 127564687f || outFromPos > 130692485f)
                            currentScore++;
                    }
                }

                // Vaf_Mean
                int vafMeanIndex = FindIndexToColumn("Vaf_Mean");
                float outVafMean;
                if (float.TryParse(currentRow[vafMeanIndex].ToString(), out outVafMean))
                {
                    if (outVafMean <= 0.03f)
                    {
                        currentScore += 1f;
                    }
                    else
                    {
                        int vafDiffIndex = FindIndexToColumn("Vaf_Differenz");
                        float outVafDiff;
                        if (float.TryParse(currentRow[vafDiffIndex].ToString(), out outVafDiff))
                        {
                            if (outVafDiff >= 0.136350466f)
                            {
                                int vafSR_Index = FindIndexToColumn("vaf_SR");
                                int vafPR_Index = FindIndexToColumn("vaf_PR");
                                float outVafSR;
                                float outVafPR;
                                if (float.TryParse(currentRow[vafSR_Index].ToString(), out outVafSR))
                                {
                                    if (outVafSR <= 0.03f)
                                        currentScore++;

                                }
                                else if (float.TryParse(currentRow[vafPR_Index].ToString(), out outVafPR))
                                {
                                    if (outVafPR <= 0.03f)
                                        currentScore++;
                                }
                            }
                        }
                    }
                }

                // Reads_Mean
                int readsMeanIndex = FindIndexToColumn("Reads_Mean");
                float outReadsMean;
                if (float.TryParse(currentRow[readsMeanIndex].ToString(), out outReadsMean))
                {
                    if (outReadsMean <= 5f) 
                    { 
                        currentScore += 1f;
                    }
                    else
                    {
                        int readsDiffIndex = FindIndexToColumn("Reads_Differenz");
                        float outReadsDif;
                        if (float.TryParse(currentRow[readsDiffIndex].ToString(), out outReadsDif))
                        {
                            if (outReadsDif >= 24.55288628f)
                            {
                                int readsSR_Index = FindIndexToColumn("tumor_alt_SR");
                                int readsPR_Index = FindIndexToColumn("tumor_alt_PR");
                                float outReadsSR;
                                float outReadsPR;
                                if (float.TryParse(currentRow[readsSR_Index].ToString(), out outReadsSR))
                                {
                                    if (outReadsSR <= 5f)
                                        currentScore++;
                                }
                                else if (float.TryParse(currentRow[readsPR_Index].ToString(), out outReadsPR))
                                {
                                    if (outReadsPR <= 5f)
                                        currentScore++;
                                }
                            }
                        }
                    }
                }


                // MYC Expression
                int mycExpressionIndex = FindIndexToColumn("MYC Expression");
                float outMycExp;
                if (float.TryParse(currentRow[mycExpressionIndex].ToString(), out outMycExp))
                {
                    if (outMycExp <= 5.32f)
                    {
                        currentScore++;
                        if (outMycExp <= 4.46f)
                            currentScore++;
                    
                            if (outMycExp <= 4.46f && (outVafMean >= 0.09f || outReadsMean >= 15f)) 
                        { 
                            currentScore++;
                            
                        }
                    }
                }
                


                // n_myl
                int nMylIndex = FindIndexToColumn("n_myl");
                float outNMyl;
                if (float.TryParse(currentRow[nMylIndex].ToString(), out outNMyl))
                {
                    currentScore += outNMyl;
                }

                // Bruchpunkte
                int bruchpunkteIndex = FindIndexToColumn("Bruchpunkt");
                string outBruchpunkte = currentRow[bruchpunkteIndex].ToString();
                if (outBruchpunkte.Equals("") || outBruchpunkte.Equals("c") || outBruchpunkte.Equals("t") )
                {
                    currentScore++;
                }

                // Partnergen
                int partnergenIndex = FindIndexToColumn("Partnergen");
                string outPartnergen = currentRow[partnergenIndex].ToString();
                if (outPartnergen.Equals(""))
                {
                    currentScore++;
                }

                SetScore(currentScore, currentRowIndex);
            }

            // Refresh data
            LoadSelectedDataTable(loadedDataSet.Tables[selectedTableIndex].Copy());
        }

        private void SetScore(float scoreToSet, int rowIndex)
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

        Dictionary<string, int> genList = new Dictionary<string, int>();
        List<Patient> patientList = new List<Patient>();

        private void btn_Analyse_Click(object sender, RoutedEventArgs e)
        {
            List<string> alreadyChecked = new List<string>();

            for (int currentRowIndex = 1; currentRowIndex < loadedDataSet_mutations.Tables[selectedTableIndex_mutations].Rows.Count; currentRowIndex++)
            {
                DataRow currentRow = loadedDataSet_mutations.Tables[selectedTableIndex_mutations].Rows[currentRowIndex];
                Patient patient = new Patient();

                patient.ArrayID = currentRow[0].ToString();
                patient.Entitaet = currentRow[2].ToString();
                

                for (int currentColumnIndex = 4; currentColumnIndex < loadedDataSet_mutations.Tables[selectedTableIndex_mutations].Columns.Count; currentColumnIndex++)
                {
                    string currentGen = currentRow[currentColumnIndex].ToString();
                    if (currentGen != "")
                    {
                        if (genList.ContainsKey(currentGen))
                        {
                            genList[currentGen]++;
                        }
                        else
                        {
                            genList.Add(currentGen, 1);
                        }

                        if (patient.GenListe.ContainsKey(currentGen))
                        {
                            patient.GenListe[currentGen]++;
                        }
                        else
                        {
                            patient.GenListe.Add(currentGen, 1);
                        }
                    }
                }

                patientList.Add(patient);
            }

            foreach (KeyValuePair<string, int> kvp in genList)
            { 
                Console.WriteLine(string.Format("{0} : {1}", kvp.Key, kvp.Value));

            }

            Console.WriteLine("Patienten: " + patientList.Count);

           
            DataTable dt = dgrid_DataGrid2.DataContext as DataTable;

            csvLines.Add("ArrayID;Entitaet;");

            foreach (KeyValuePair<string, int> kvp in genList)
            {
                csvLines[0] += kvp.Key + ";";
            }

            foreach (Patient p in patientList)
            {

                DataRow dr = dt.NewRow();
                csvLines.Add( p.ArrayID + ";" + p.Entitaet + ";");

                foreach (KeyValuePair<string, int> kvp in genList) 
                {
                    if (p.GenListe.ContainsKey(kvp.Key))
                    {
                        csvLines[csvLines.Count-1] += p.GenListe[kvp.Key];
                    } 
                    
                    csvLines[csvLines.Count - 1] += ";";
                }
            }

            WriteCSV(csvLines.ToArray());
        }  
        
        private void WriteCSV(string[] lines)
        {
            System.IO.File.WriteAllLines("data.csv", lines);
        }
    }
}
