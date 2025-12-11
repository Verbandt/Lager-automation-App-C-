using ClosedXML.Excel;
using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Windows;

namespace Lager_automation.Models
{
    public class ExcelHandler
    {

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

        public DataTable ToDataTable(WorkSheet sheet)
        {
            var dt = new DataTable();

            // Header row (row 0)
            var header = sheet.Rows[0];
            foreach (var cell in header.Columns)
            {
                dt.Columns.Add(cell.StringValue);
            }

            int rowCount = sheet.Rows.Count();

            // Data rows
            for (int r = 1; r < rowCount; r++)
            {
                var row = sheet.Rows[r];
                var dataRow = dt.NewRow();

                int colCount = row.Columns.Count();

                for (int c = 0; c < colCount; c++)
                {
                    dataRow[c] = row.Columns[c].Value ?? DBNull.Value;
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
                XLWorkbook workbook = XLWorkbook.Load(path);
                WorkSheet worksheet = workbook.WorkSheets[0];
                var dt = ToDataTable(workbook.WorkSheets.First());
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

            bool dtIsCorrect = VerifyFileContent(dt);
            if (!dtIsCorrect)
                MessageBox.Show("Excel-filen har inte rätt format eller saknar nödvändiga kolumner.");
                return false;


        }

        private bool VerifyFileContent(DataTable dt)
        {
            return true;
        }

    }
}
