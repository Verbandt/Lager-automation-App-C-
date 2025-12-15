using ClosedXML.Excel;
using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Windows;
using Azure.Identity;
using Microsoft.Graph;
using System.Threading.Tasks;


namespace Lager_automation.Models
{
    public class ExcelHandler
    {
        
        private DataTable? ArticlesTable;
        private CancellationTokenSource? _cts;
        public bool IsBusy { get; private set; }
        private Task? _refreshTask;
        public Task? RefreshTask => _refreshTask;

        public DataTable? RacksPartsDt { get; set; }
        public DataTable? ArticleInfoDt { get; set; }

        const string AritcleExcelPath = @"\\olognmhm01.olo.volvocars.net\PROJ\9406_Logistic_Layout\PSCE\KiMö\Lager automation\artikeldata.xlsx";
        const string RackspartsExcelPath = @"\\olognmhm01.olo.volvocars.net\PROJ\9406_Logistic_Layout\PSCE\KiMö\Lager automation\stallage lista.xlsx";


        private string? SelectedFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Välj Excel-fil",
                Filter = "Excel Files|*.xlsx;*.xls;*.xlsm",
                Multiselect = false
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        public static DataTable ToDataTable(IXLWorksheet sheet)
        {
            var dt = new DataTable();

            var headerRow = sheet.FirstRowUsed();
            if (headerRow == null)
                return dt;
            foreach (var cell in headerRow.Cells())
                dt.Columns.Add(cell.GetValue<string>());

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                DataRow dataRow = dt.NewRow();
                int i = 0;
                foreach (var cell in row.Cells(1, dt.Columns.Count))
                {
                    dataRow[i++] = cell.GetValue<string>();
                }

                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        public DataTable? ImportExcelFile()
        {
            string? path = SelectedFile();
            if (path == null)
                return null;

            try
            {
                XLWorkbook workbook = new(path);
                IXLWorksheet worksheet = workbook.Worksheet(1);

                DataTable dt = ToDataTable(worksheet);
                return dt;
            }

            catch (IOException ioEx) when
                (ioEx.Message.Contains("used by another process") ||
                 ioEx.Message.Contains("access") ||
                 ioEx.HResult == -2147024864)
            {
                MessageBox.Show("Filen är redan öppen i ett annat program. Vänligen stäng filen och försök igen.");
                return null;
            }
            catch (Exception ex)
            {

                MessageBox.Show("Fel vid inläsning av Excel-fil: " + ex.Message);
                return null;
            }
        }

        public bool LoadExcelFile()
        {
            DataTable? dt = ImportExcelFile();

            if (dt == null)
                return false;

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Excel-filen är tom.");
                return false;
            }

            bool dtIsCorrect = VerifyFileContent(dt);
            if (!dtIsCorrect)
                MessageBox.Show("Excel-filen har inte rätt format eller saknar nödvändiga kolumner.");
            return false;


        }

        private bool VerifyFileContent(DataTable dt)
        {
            return true;
        }

        public async Task LoadExcelInfoAsync()
        {
            IsBusy = true;
            try
            {
                IEnumerable<string> excelPaths = [AritcleExcelPath, RackspartsExcelPath];
                // 🔹 1) REFRESH — SEQUENTIAL (Excel Interop)
                foreach (var path in excelPaths)
                {
                    await Task.Run(() =>
                    {
                        var thread = new Thread(() =>
                        {
                            string fileName = Path.GetFileName(path).ToLower();
                            if (fileName.Contains("artikeldata"))
                            {
                                ArticleInfoDt = ImportInfoFromExcelFIles.RefreshAndReadExcel_WithLockWait(path);
                            }
                            else if (fileName.Contains("stallage lista"))
                            {
                                RacksPartsDt = ImportInfoFromExcelFIles.RefreshAndReadExcel_WithLockWait(path);
                            }
                        });

                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        thread.Join();
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool TryCancel()
        {
            if (IsBusy && _cts != null && !_cts.IsCancellationRequested)
            {
                // Only works BEFORE Excel Interop actually runs
                _cts.Cancel();
                return true;
            }

            return false;
        }

    }
}
