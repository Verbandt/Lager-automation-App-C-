using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public static class RacksCost
    {
        public static Dictionary<string, Part> Parts { get; set; } = PartsImporter.ImportParts("C:\\Users\\KMOLLER2\\OneDrive - Volvo Cars\\Desktop\\Johannes hatar HF\\Lager automation\\Stallage lista.xlsx");

        public static void AddShelfLevel(bool backCover, string typeOfBeam, int beamLength)
        {
            int deckingReduction = beamLength switch
            {
                1900 => -2,
                3400 => -1,
                _ => 0
            };

            Parts[typeOfBeam].AddPart();
            Parts["beam_lock"].AddPart();
            Parts["decking"].AddPart(deckingReduction);

            if (backCover)
            {
                Parts["back_cover_add_on"].AddPart();
            }
        }

        public static void AddNewSection(bool backCover, string typeOfFrame, bool doubleFrames)
        {
            int beamsToAdd = doubleFrames ? 2 : 0;
            int workTimeToAdd = doubleFrames ? 1 : 0;

            Parts[typeOfFrame].AddPart(beamsToAdd);
            Parts["work_time"].AddPart(workTimeToAdd);
            Parts["pillar_protection"].AddPart();

            if(backCover)
            {
                Parts["back_cover_1"].AddPart();
                Parts["back_cover_2"].AddPart();
                if (doubleFrames)
                {
                    Parts["joint_sleeve"].AddPart();
                }
            }
        }

        public static double TotalCost()
        {
            return Parts.Values.Sum(part => part.TotalCost());
        }

        public static void ExportToExcel(string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Racks Cost");
            ws.Cell(1, 1).Value = "Part Name";
            ws.Cell(1, 2).Value = "Count";
            ws.Cell(1, 3).Value = "Price/ SEK";
            ws.Cell(1, 4).Value = "Total Cost";
            int row = 2;
            foreach (var part in Parts.Values)
            {
                ws.Cell(row, 2).Value = part.PartName;
                ws.Cell(row, 3).Value = part.Count;
                ws.Cell(row, 4).Value = part.Price;
                ws.Cell(row, 5).Value = part.TotalCost();
                row++;
            }
            ws.Cell(row, 6).Value = "Grand Total:";
            ws.Cell(row, 7).Value = TotalCost();
            workbook.SaveAs(filePath);
        }

    }
}
