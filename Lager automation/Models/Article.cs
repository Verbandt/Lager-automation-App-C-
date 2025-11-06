using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class Article
    {
        public string ArticleNumber { get; set; }
        public string Customer { get; set; }
        public string EmbName { get; set; }
        public string EmbType { get; set; }
        public string Factory { get; set; }
        public string CommonPart { get; set; }

        public int FillRate { get; set; }
        public int EmbLength { get; set; }
        public int EmbWidth { get; set; }
        public int EmbHeight { get; set; }
        public int EmbNeeded { get; set; }
        public int BruttoWeight { get; set; }

        public bool GEmb { get; set; }

        public Article(string articleNumber, string customer, string embName, int embLength, int embWidth, int embHeight, int embNeeded, int bruttoWeight, 
                        int fillRate, string commonPart,  bool gEmb, string factory)
        {
            ArticleNumber = articleNumber;
            Customer = customer;
            EmbName = embName;
            EmbLength = embLength;
            EmbWidth = embWidth;
            EmbHeight = embHeight;
            EmbNeeded = embNeeded;
            BruttoWeight = bruttoWeight;
            FillRate = fillRate;
            CommonPart = commonPart;
            GEmb = gEmb;
            Factory = factory;

            EmbType = CheckEmbType();
            RotateEmbIfNeeded();
            CheckIfCommonPart();
        }

        private string CheckEmbType()
        {
            if (EmbName.ToLower().Contains("b"))
            {
                return "plastic pallet";
            }
            else if(EmbName.ToLower().Contains("v"))
            {
                return "paper pallet";
            }
            else
            {
                return "wood pallet";
            }
        }

        private void RotateEmbIfNeeded()
        {
            if (EmbLength > 1225 && EmbLength != 1450 && EmbLength != 1491)
            {
                (EmbLength, EmbWidth) = (EmbWidth, EmbLength);
            }
        }

        private void CheckIfCommonPart()
        {
            if (CommonPart.ToLower().Contains("ja"))
            {
                Factory = "Common";
            }
        }
    }
}
