using IronXL;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void LoadExcelFile()
        {
            string? path = SelectedFile();
            if (path == null)
                return;

            try
            {
                WorkBook workbook = WorkBook.Load(path);
                WorkSheet worksheet = workbook.WorkSheets[0];
            }

            catch (IOException ioEx) when
                (ioEx.Message.Contains("used by another process") ||
                 ioEx.Message.Contains("access") ||
                 ioEx.HResult == -2147024864)
            {
                   MessageBox.Show("Filen är redan öppen i ett annat program. Vänligen stäng filen och försök igen.");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Fel vid inläsning av Excel-fil: " + ex.Message);
            }
            
        }


        private void VerifyFileContent(WorkSheet worksheet)
        {
            
        }
    }
}
