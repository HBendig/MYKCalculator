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
        private string dataPath_mutations2 = "";
        private string dataPath_mutationsTemplate = "";
        private string dataPath_mutationsTemplate2 = "";

        private DataSet loadedDataSet;
        private DataSet loadedDataSet_mutations;
        private DataSet loadedDataSet_mutations2;
        private DataSet loadedDataSet_mutationsTemplate;
        private DataSet loadedDataSet_mutationsTemplate2;
        private int selectedTableIndex;
        private int selectedTableIndex_mutations;
        private int selectedTableIndex_mutations2;
        private int selectedTableIndex_mutationsTemplate;
        private int selectedTableIndex_mutationsTemplate2;

        private List<string> csvLines = new List<string>();
        private Dictionary<string, int> genList = new Dictionary<string, int>();
        private List<Patient> patientList = new List<Patient>();


        public MainWindow()
        {
            InitializeComponent();

            cmbbox_TableChooser.SelectionChanged += Cmbbox_TableChooser_SelectionChanged;
            cmbbox_TableChooser2.SelectionChanged += Cmbbox_TableChooser_mutations_SelectionChanged;
            cmbbox_TableChooser3.SelectionChanged += Cmbbox_TableChooser_mutations2_SelectionChanged;
            cmbbox_TableChooserTemplate3.SelectionChanged += Cmbbox_TableChooser_mutationsTemplate2_SelectionChanged;
            cmbbox_TableChooserTemplate2.SelectionChanged += Cmbbox_TableChooser_mutationsTemplate_SelectionChanged;

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
        private void Cmbbox_TableChooser_mutations2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));
            selectedTableIndex_mutations2 = cmbbox_TableChooser3.SelectedIndex;
            LoadSelectedDataTable_mutations2(loadedDataSet_mutations2.Tables[selectedTableIndex_mutations2].Copy());
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void Cmbbox_TableChooser_mutationsTemplate2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));
            selectedTableIndex_mutationsTemplate2 = cmbbox_TableChooserTemplate3.SelectedIndex;
            LoadSelectedDataTable_mutationsTemplate2(loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Copy());
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void Cmbbox_TableChooser_mutationsTemplate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));
            selectedTableIndex_mutationsTemplate = cmbbox_TableChooserTemplate2.SelectedIndex;
            //  LoadSelectedDataTable_mutationsTemplate(loadedDataSet_mutationsTemplate.Tables[selectedTableIndex_mutationsTemplate].Copy());
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
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
                if (dataPath_mutationsTemplate != "")
                    btn_ImportData2.IsEnabled = true;
            }
        }
        private void btn_xlsPickerButton3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                dataPath_mutations2 = lbl_Filepath3.Text = openFileDialog.FileName;
                
                if(dataPath_mutationsTemplate2 != "")
                    btn_ImportData3.IsEnabled = true;
            }
        }
        private void btn_xlsPickerButtonTemplate3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                dataPath_mutationsTemplate2 = lbl_Filepath_template3.Text = openFileDialog.FileName;
                if(dataPath_mutations2 != "")
                    btn_ImportData3.IsEnabled = true;
            }
        }

        private void btn_ImportData_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData));
            importThread.Start();
        }
        private void btn_ImportData2_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData_Mutations));
            importThread.Start();
        }
        private void btn_ImportData3_Click(object sender, RoutedEventArgs e)
        {
            Thread importThread = new Thread(new ThreadStart(ImportXLSData_Mutations2));
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

            // Template
            using (var stream = File.Open(dataPath_mutationsTemplate, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    loadedDataSet_mutationsTemplate = reader.AsDataSet();
                }
            }

            // Combobox zur Tabellenwahl füllen
            List<string> tableNames_template = new List<string>();
            foreach (DataTable dataTable in loadedDataSet_mutationsTemplate.Tables)
            {
                tableNames_template.Add(dataTable.TableName);
            }
           

            Dispatcher.Invoke((() => FillCombobox2(tableNames)));
            Dispatcher.Invoke(() => FillComboboxTemplate2(tableNames_template));
            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void ImportXLSData_Mutations2()
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            // Datasrc
            using (var stream = File.Open(dataPath_mutations2, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    loadedDataSet_mutations2 = reader.AsDataSet();
                }
            }

            // Combobox zur Tabellenwahl füllen
            List<string> tableNames = new List<string>();
            foreach (DataTable dataTable in loadedDataSet_mutations2.Tables)
            {
                tableNames.Add(dataTable.TableName);
            }

            // Template
            using (var stream = File.Open(dataPath_mutationsTemplate2, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    loadedDataSet_mutationsTemplate2 = reader.AsDataSet();
                }
            }

            // Combobox zur Tabellenwahl füllen
            List<string> tableNames_template = new List<string>();
            foreach (DataTable dataTable in loadedDataSet_mutationsTemplate2.Tables)
            {
                tableNames_template.Add(dataTable.TableName);
            }
            // Template reseten
            templateDic = null;

            Dispatcher.Invoke((() => FillCombobox3(tableNames)));
            Dispatcher.Invoke(()  => FillComboboxTemplate3(tableNames_template));
            Dispatcher.Invoke(()  => chkbx_SingleCount.IsEnabled = true);
            Dispatcher.Invoke(()  => ShowLoadingScreen(false));
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
        private void FillCombobox3(List<string> tableNames)
        {
            cmbbox_TableChooser3.ItemsSource = tableNames;
            cmbbox_TableChooser3.IsEnabled = true;
            cmbbox_TableChooser3.SelectedIndex = 0;
        }
        private void FillComboboxTemplate3(List<string> tableNames)
        {
            cmbbox_TableChooserTemplate3.ItemsSource = tableNames;
            cmbbox_TableChooserTemplate3.IsEnabled = true;
            cmbbox_TableChooserTemplate3.SelectedIndex = 0;
        }
        private void FillComboboxTemplate2(List<string> tableNames)
        {
            cmbbox_TableChooserTemplate2.ItemsSource = tableNames;
            cmbbox_TableChooserTemplate2.IsEnabled = true;
            cmbbox_TableChooserTemplate2.SelectedIndex = 0;
        }

        private void LoadSelectedDataTable(DataTable loadedTable)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            dgrid_DataGrid.DataContext = loadedTable;
            SetColumnNames(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            btn_CalculateScore.IsEnabled = true;

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void LoadSelectedDataTable_mutations(DataTable loadedTable)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            dgrid_DataGrid2.DataContext = loadedTable;
            SetColumnNames_mutations(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            btn_Analyse.IsEnabled = true;

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void LoadSelectedDataTable_mutations2(DataTable loadedTable)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));
            dgrid_DataGrid3.DataContext = loadedTable;
            SetColumnNames_mutations2(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            // btn_ReadData.IsEnabled = false;
            //if(templateDic == null)
            //    btn_SetTemplate.IsEnabled = true;

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void LoadSelectedDataTable_mutationsTemplate2(DataTable loadedTable)
        {
            ShowLoadingScreen(true);
            btn_SetTemplate.IsEnabled = false;
            dgrid_DataGridTemplate3.DataContext = loadedTable;
            SetColumnNames_mutationsTemplate2(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            if (templateDic == null)
                btn_SetTemplate.IsEnabled = true;

            ShowLoadingScreen(false);
        }

        private void LoadSelectedDataTable_mutationsTemplate(DataTable loadedTable)
        {
            ShowLoadingScreen(true);
           // btn_SetTemplate.IsEnabled = false;
           // dgrid_DataGridTemplate3.DataContext = loadedTable;
          //  SetColumnNames_mutationsTemplate2(loadedTable);

            loadedTable.Rows[0].Delete();
            loadedTable.AcceptChanges();
            if (templateDic == null)
                btn_SetTemplate.IsEnabled = true;

            ShowLoadingScreen(false);
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
        private void SetColumnNames_mutations2(DataTable data)
        {
            for (int n = 0; n < dgrid_DataGrid3.Columns.Count; n++)
            {
                dgrid_DataGrid3.Columns[n].Header = data.Rows[0].ItemArray[n];
            }
        }
        private void SetColumnNames_mutationsTemplate2(DataTable data)
        {
            for (int n = 0; n < dgrid_DataGridTemplate3.Columns.Count; n++)
            {
                dgrid_DataGridTemplate3.Columns[n].Header = data.Rows[0].ItemArray[n];
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
                int singleColumnIndex = FindIndexToColumn("single", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                if (currentRow[singleColumnIndex].ToString() == "1")
                {
                    currentScore++;
                    currentScore++;
                }

                // from Pos
                int fromPosIndex = FindIndexToColumn("from_pos", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
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
                int vafMeanIndex = FindIndexToColumn("Vaf_Mean", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                float outVafMean;
                if (float.TryParse(currentRow[vafMeanIndex].ToString(), out outVafMean))
                {
                    if (outVafMean <= 0.03f)
                    {
                        currentScore += 1f;
                    }
                    else
                    {
                        int vafDiffIndex = FindIndexToColumn("Vaf_Differenz", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                        float outVafDiff;
                        if (float.TryParse(currentRow[vafDiffIndex].ToString(), out outVafDiff))
                        {
                            if (outVafDiff >= 0.136350466f)
                            {
                                int vafSR_Index = FindIndexToColumn("vaf_SR", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                                int vafPR_Index = FindIndexToColumn("vaf_PR", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
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
                int readsMeanIndex = FindIndexToColumn("Reads_Mean", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                float outReadsMean;
                if (float.TryParse(currentRow[readsMeanIndex].ToString(), out outReadsMean))
                {
                    if (outReadsMean <= 5f) 
                    { 
                        currentScore += 1f;
                    }
                    else
                    {
                        int readsDiffIndex = FindIndexToColumn("Reads_Differenz", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                        float outReadsDif;
                        if (float.TryParse(currentRow[readsDiffIndex].ToString(), out outReadsDif))
                        {
                            if (outReadsDif >= 24.55288628f)
                            {
                                int readsSR_Index = FindIndexToColumn("tumor_alt_SR", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                                int readsPR_Index = FindIndexToColumn("tumor_alt_PR", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
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
                int mycExpressionIndex = FindIndexToColumn("MYC Expression", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
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
                int nMylIndex = FindIndexToColumn("n_myl", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                float outNMyl;
                if (float.TryParse(currentRow[nMylIndex].ToString(), out outNMyl))
                {
                    currentScore += outNMyl;
                }

                // Bruchpunkte
                int bruchpunkteIndex = FindIndexToColumn("Bruchpunkt", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
                string outBruchpunkte = currentRow[bruchpunkteIndex].ToString();
                if (outBruchpunkte.Equals("") || outBruchpunkte.Equals("c") || outBruchpunkte.Equals("t") )
                {
                    currentScore++;
                }

                // Partnergen
                int partnergenIndex = FindIndexToColumn("Partnergen", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
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
            int scoreColumnIndex = FindIndexToColumn("Score", loadedDataSet.Tables[selectedTableIndex].Rows[0].ItemArray);
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].ItemArray[scoreColumnIndex] = scoreToSet;
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].SetField(scoreColumnIndex, scoreToSet);
            loadedDataSet.Tables[selectedTableIndex].Rows[rowIndex].AcceptChanges();
        }

        private int FindIndexToColumn(string searchKeyword, object[] data)
        {
            List<string> headerList = new List<string>();
            foreach (object obj in data)
            {
                headerList.Add(obj.ToString());
            }

            return headerList.FindIndex(x => x.Equals(searchKeyword));
        }

        private void btn_CalculateScore_Click(object sender, RoutedEventArgs e)
        {
            CalculateScore();
           
        }


        private void btn_Analyse_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            // Template Gen-Liste einlesen
            genList = new Dictionary<string, int>();
            DataRow currentRow_headline = loadedDataSet_mutationsTemplate.Tables[selectedTableIndex_mutationsTemplate].Rows[0];

            for (int currentColumnIndex = 4; currentColumnIndex < loadedDataSet_mutationsTemplate.Tables[selectedTableIndex_mutationsTemplate].Columns.Count; currentColumnIndex++)
            {
                string currentGen = currentRow_headline[currentColumnIndex].ToString();
                if (currentGen != "" && !genList.ContainsKey(currentGen))
                {
                    genList.Add(currentGen, 0);
                }
            }

            // Patienten hinzufügen
            for (int currentRowIndex = 1; currentRowIndex < loadedDataSet_mutationsTemplate.Tables[selectedTableIndex_mutationsTemplate].Rows.Count; currentRowIndex++)
            {
                DataRow currentRow = loadedDataSet_mutationsTemplate.Tables[selectedTableIndex_mutationsTemplate].Rows[currentRowIndex];
                Patient patient = new Patient();

                patient.ArrayID = currentRow[0].ToString();
                patient.Entitaet = currentRow[1].ToString();


                patient.GenListe = new Dictionary<string, int>(genList);

                patientList.Add(patient);
            }

         
            Console.WriteLine("Patienten: " + patientList.Count);

            foreach(DataTable dt in loadedDataSet_mutations.Tables)
            {
                int symbolIndex = FindIndexToColumn("symbol", dt.Rows[0].ItemArray);
                int patientIdIndex = FindIndexToColumn("array_id", dt.Rows[0].ItemArray);
                if(!symbolIndex.Equals(-1) && !patientIdIndex.Equals(-1))
                {
                    for (int currentRowIndex = 1; currentRowIndex < dt.Rows.Count; currentRowIndex++)
                    {
                        DataRow currentRow = dt.Rows[currentRowIndex];

                        string patientID = currentRow[patientIdIndex].ToString();
                        if (patientID != "-1")
                        {
                            string symbol = currentRow[symbolIndex]?.ToString();

                            foreach (Patient p in patientList)
                            {
                                if (p.ArrayID == patientID)
                                {
                                    if (p.GenListe.ContainsKey(symbol))
                                    {
                                        p.GenListe[symbol]++;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

            }



            // CSV erstellen
            csvLines.Add("ArrayID;Entitaet;");

            foreach (KeyValuePair<string, int> kvp in genList)
            {
                csvLines[0] += kvp.Key + ";";
            }

            foreach (Patient p in patientList)
            {
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
            // .. und schreiben
            WriteCSV(csvLines.ToArray());

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }
        private void btn_Analyse2_Click(object sender, RoutedEventArgs e)
        {

            for (int currentRowIndex = 1; currentRowIndex < loadedDataSet_mutations2.Tables[selectedTableIndex_mutations].Rows.Count; currentRowIndex++)
            {
                DataRow currentRow = loadedDataSet_mutations2.Tables[selectedTableIndex_mutations].Rows[currentRowIndex];
                Patient patient = new Patient();

                patient.ArrayID = currentRow[0].ToString();
                patient.Entitaet = currentRow[2].ToString();


                for (int currentColumnIndex = 4; currentColumnIndex < loadedDataSet_mutations2.Tables[selectedTableIndex_mutations].Columns.Count; currentColumnIndex++)
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


            DataTable dt = dgrid_DataGrid3.DataContext as DataTable;

            csvLines.Add("ArrayID;Entitaet;");

            foreach (KeyValuePair<string, int> kvp in genList)
            {
                csvLines[0] += kvp.Key + ";";
            }

            foreach (Patient p in patientList)
            {

                DataRow dr = dt.NewRow();
                csvLines.Add(p.ArrayID + ";" + p.Entitaet + ";");

                foreach (KeyValuePair<string, int> kvp in genList)
                {
                    if (p.GenListe.ContainsKey(kvp.Key))
                    {
                        csvLines[csvLines.Count - 1] += p.GenListe[kvp.Key];
                    }

                    csvLines[csvLines.Count - 1] += ";";
                }
            }

            WriteCSV(csvLines.ToArray());
        }

        private void WriteCSV(string[] lines)
        {
            System.IO.File.Delete("data.csv");
            System.IO.File.WriteAllLines("data.csv", lines);
        }

        private Dictionary<string, Dictionary<string, int>> ResultDataset = new Dictionary<string, Dictionary<string, int>>();
        private void btn_ReadData_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));
            List<string> patientAlreadyCounted = new List<string>();
            Dictionary<string, int> countList = new Dictionary<string, int>( templateDic );
            for (int currentRowIndex = 1; currentRowIndex < loadedDataSet_mutations2.Tables[selectedTableIndex_mutations2].Rows.Count; currentRowIndex++)
            {
                DataRow currentRow = loadedDataSet_mutations2.Tables[selectedTableIndex_mutations2].Rows[currentRowIndex];
                int index = FindIndexToColumn("symbol", loadedDataSet_mutations2.Tables[selectedTableIndex_mutations2].Rows[0].ItemArray);
                string currentPatientID = currentRow[FindIndexToColumn("array_id", loadedDataSet_mutations2.Tables[selectedTableIndex_mutations2].Rows[0].ItemArray)].ToString();
                string currentEntity = currentRow[index].ToString();

                if (countList.ContainsKey(currentEntity))
                {
                    if ((bool)chkbx_SingleCount.IsChecked)
                    {
                        string combination = currentPatientID + "_" + currentEntity;
                        if (!patientAlreadyCounted.Contains(combination))
                        {
                            patientAlreadyCounted.Add(combination);
                            countList[currentEntity] += 1;
                        }
                    }
                    else
                    {
                        countList[currentEntity] += 1;
                    }
                }
            }

            if(!ResultDataset.ContainsKey(cmbbox_TableChooser3.Text))
                ResultDataset.Add(cmbbox_TableChooser3.Text, countList);

            DataRow headlineRow = loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Rows[0];
            for (int currentRowIndex = 1; currentRowIndex < loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Rows.Count; currentRowIndex++)
            {
                DataRow currentRow = loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Rows[currentRowIndex];
                string currentEntity = currentRow[0].ToString();
                if (currentEntity == cmbbox_TableChooser3.Text)
                {
                    for (int currentColumnIndex = 2; currentColumnIndex < loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Columns.Count; currentColumnIndex++)
                    {
                        if(headlineRow[currentColumnIndex].ToString() != "")
                            currentRow[currentColumnIndex] = countList[headlineRow[currentColumnIndex].ToString()];
                    }
                }
            }


            // Refresh data
            LoadSelectedDataTable_mutationsTemplate2(loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Copy());

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }

        private Dictionary<string, int> templateDic; 
        private void btn_SetTemplate_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => ShowLoadingScreen(true));

            ResultDataset = new Dictionary<string, Dictionary<string, int>>();

            DataRow currentRow = loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Rows[0];
            templateDic = new Dictionary<string, int>();

            for (int currentColumnIndex = 2; currentColumnIndex < loadedDataSet_mutationsTemplate2.Tables[selectedTableIndex_mutationsTemplate2].Columns.Count; currentColumnIndex++)
            {
                string currentEntity = currentRow[currentColumnIndex].ToString();
                if (currentEntity != "")
                {
                    if (!templateDic.ContainsKey(currentEntity))
                    {
                        templateDic.Add(currentEntity, 0);
                    }
                }
            }
            
            btn_ReadData.IsEnabled = true;
            btn_SetTemplate.IsEnabled = false;

            Dispatcher.Invoke(() => ShowLoadingScreen(false));
        }

        private void btn_xlsPickerButtonTemplate2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Worksheets|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                dataPath_mutationsTemplate = lbl_Filepath_template2.Text = openFileDialog.FileName;
                if (dataPath_mutations != "")
                    btn_ImportData2.IsEnabled = true;
            }
        }
    }
}
