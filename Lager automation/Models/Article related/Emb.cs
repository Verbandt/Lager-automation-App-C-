using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class Emb(int x, int y, int length, int height, string articleNumber, string embName, string embType)
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int Length { get; set; } = length;
        public int Height { get; set; } = height;
        public string ArticleNumber { get; set; } = articleNumber;
        public string EmbName { get; set; } = embName;
        public string EmbType { get; set; } = embType;

        public (int r, int g, int b) EmbColor => SetColor();

        private (int r, int g, int b) SetColor()
        {
            if (EmbType == "plastic pallet")
            {
                return (45, 94, 214);
            }
            else if (EmbType == "paper pallet")
            {
                return (197, 39, 245);
            }
            else
            {
                return (217, 168, 43); // Light Gray for unknown types
            }

        }

    }
}
