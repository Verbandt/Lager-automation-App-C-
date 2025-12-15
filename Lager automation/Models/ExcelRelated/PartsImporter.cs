using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Lager_automation.Models
{
    public static class PartsImporter
    {
        public static Dictionary<string, Part> ImportParts(DataTable dt)
        {
            var parts = new Dictionary<string, Part>();

            if (dt == null || dt.Columns.Count == 0)
            {
                Console.WriteLine("DataTable is empty or null!");
                return parts;
            }

            // 🔹 Build header lookup (column name → index)
            var headers = dt.Columns
                .Cast<DataColumn>()
                .ToDictionary(
                    c => c.ColumnName.Trim(),
                    c => c.Ordinal
                );

            // 🔹 Required columns (fail fast & clearly)
            string[] requiredColumns =
            {
               "Kod namn",
               "Benämning",
               "Pris/ SEK",
               "Racks del",
               "Antal",
               "Kategori"
            };

            foreach (var col in requiredColumns)
            {
                if (!headers.ContainsKey(col))
                    throw new Exception($"Missing required column: {col}");
            }

            // 🔹 Iterate rows
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                string codeName = row[headers["Kod namn"]]?.ToString()?.Trim() ?? "";
                string partName = row[headers["Benämning"]]?.ToString()?.Trim() ?? "";
                string belongsTo = row[headers["Racks del"]]?.ToString()?.Trim() ?? "";
                string category = row[headers["Kategori"]]?.ToString()?.Trim() ?? "";

                double price = row[headers["Pris/ SEK"]] != DBNull.Value
                    ? Convert.ToDouble(row[headers["Pris/ SEK"]])
                    : 0;

                int quantity = row[headers["Antal"]] != DBNull.Value
                    ? Convert.ToInt32(row[headers["Antal"]])
                    : 0;

                if (string.IsNullOrWhiteSpace(codeName))
                    continue; // skip invalid rows

                var part = new Part(codeName, partName, belongsTo, price, quantity, category);
                parts[codeName] = part;
            }

            return parts;
        }
    }
}
