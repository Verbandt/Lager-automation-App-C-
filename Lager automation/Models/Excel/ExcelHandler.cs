using IronXL;
using Microsoft.Win32;
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


        public WorkSheet? ImportExcelFile()
        {
            string? path = SelectedFile();
            if (path == null)
                return null;

            try
            {
                WorkBook workbook = WorkBook.Load(path);
                WorkSheet worksheet = workbook.WorkSheets[0];
                return worksheet;
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
            WorkSheet? worksheet = ImportExcelFile();

            if (worksheet == null)
                return false;

            VerifyFileContent(worksheet);


            return true; // la in för att inte crasha programmet under testning


        }
        private void VerifyFileContent(WorkSheet worksheet)
        {
            
        }
    }
}
