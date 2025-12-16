using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace Lager_automation.Models
{
    class ImportInfoFromExcelFIles
    {
        
        const int RefreshTimeoutSeconds = 120;

        public static bool IsFileLocked(string path)
        {
            try
            {
                using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.None);

                return false; // unlocked
            }
            catch (IOException)
            {
                return true; // locked
            }
        }


        private async void LoadFromNetWorkDrive()
        {

        }

        public static void RefreshExcelFile(string path)
        {
            Excel.Application excel = null;
            Excel.Workbook workbook = null;

            try
            {
                excel = new Excel.Application
                {
                    Visible = false,
                    DisplayAlerts = false
                };

                workbook = excel.Workbooks.Open(
                    path,
                    ReadOnly: false,
                    Editable: true);

                workbook.RefreshAll();

                // IMPORTANT: wait for Power Query / async connections
                excel.CalculateUntilAsyncQueriesDone();

                workbook.Save();
            }
            finally
            {
                workbook?.Close(false);
                excel?.Quit();

                // Release COM objects
                if (workbook != null) Marshal.ReleaseComObject(workbook);
                if (excel != null) Marshal.ReleaseComObject(excel);
            }
        }

        public static void WaitForFile(string path, int timeoutSeconds)
        {
            var start = DateTime.Now;

            while (IsFileLocked(path))
            {
                if ((DateTime.Now - start).TotalSeconds > timeoutSeconds)
                    throw new TimeoutException("Excel file is still locked.");

                Thread.Sleep(500);
            }
        }

        public static DataTable ReadExcelWithClosedXml(string path)
        {
            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(1);

            return ExcelHandler.ToDataTable(sheet);
        }

        public static DataTable RefreshAndReadExcel(string path)
        {
            // 1. Refresh the file
            RefreshExcelFile(path);

            // 2. Wait until Excel releases the lock
            WaitForFile(path, RefreshTimeoutSeconds);

            // 3. Read updated data
            return ReadExcelWithClosedXml(path);
        }

        public static DataTable RefreshAndReadExcel_WithLockWait(string path)
        {
            // 1️⃣ Wait for unlock (up to 60s)
            bool unlocked = WaitForFileUnlock(path, TimeSpan.FromSeconds(60));

            if (!unlocked)
            {
                MessageBox.Show(
                "Exclfilen är låst.\n" +
                "Försökte i en minut",
                "Fel",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }

            //Safe to proceed
            return RefreshAndReadExcel(path);
        }

        public static bool WaitForFileUnlock(string path, TimeSpan timeout, int pollMilliseconds = 500)
        {
            var start = DateTime.UtcNow;

            while (IsFileLocked(path))
            {
                if (DateTime.UtcNow - start > timeout)
                    return false; // timed out

                Thread.Sleep(pollMilliseconds);
            }

            return true; // unlocked
        }
    }
}
