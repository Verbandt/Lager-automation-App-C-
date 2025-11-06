using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Lager_automation.Models
{
    public static class PartsImporter
    {
        public static Dictionary<string, Part> ImportParts(string filePath)
        {
            var parts = new Dictionary<string, Part>();

            using var workbook = new XLWorkbook(filePath);

            var ws = workbook.Worksheets.FirstOrDefault();
            if (ws == null)
            {
                Console.WriteLine("No worksheet found!");
                return parts;
            }

            var headerRow = ws.FirstRowUsed();
            if (headerRow == null)
            {
                Console.WriteLine("No header row found!");
                return parts;
            }

            var headers = headerRow.CellsUsed()
                .Select((cell, index) => new { Name = cell.GetString().Trim(), Index = index + 1 })
                .ToDictionary(h => h.Name, h => h.Index);

            var rows = ws.RowsUsed().Skip(1); // Skip header row

            foreach (var row in rows)
            {
                string codeName = row.Cell(headers["Kod namn"]).GetString();
                string partName = row.Cell(headers["Benämning"]).GetString();
                double price = row.Cell(headers["Pris/ SEK"]).GetDouble();
                string belongsTo = row.Cell(headers["Racks del"]).GetString();
                int quantity = row.Cell(headers["Antal"]).GetValue<int>();

                var part = new Part(codeName, partName, belongsTo, price, quantity);
                parts[codeName] = part;
            }
            return parts;
        }
    }
}
