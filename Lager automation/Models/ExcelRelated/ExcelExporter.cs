using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public static class ExcelExporter
    {
        public static bool ExportToExcel(DataTable table, string defaultFileName = "Export.xlsx")
        {
            if (table == null || table.Rows.Count == 0)
            {
                return false;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Spara Excel-fil",
                Filter = "Excel Files|*.xlsx",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() != true)
                return false;

            using XLWorkbook Workbook = new();
            IXLWorksheet worksheet = Workbook.Worksheets.Add("Data");

            var xlTable = worksheet.Cell(1, 1).InsertTable(table, true);
            xlTable.Name = "Modifierad ArtikelTabell";

            worksheet.Columns().AdjustToContents();
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.SheetView.FreezeRows(1);

            Workbook.SaveAs(saveFileDialog.FileName);
            return true;
        }
    }
}
