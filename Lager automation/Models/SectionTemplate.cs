using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class SectionTemplate: ITemplate
    {
        public List<Part> Parts { get; set; } = new();

        public string Name { get; set; } = string.Empty;
        public string Frame { get; set; } = string.Empty;
        public string Beam { get; set; } = string.Empty;
        public string BackCover { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;

        public SectionTemplate()
        {
            ExtractPartNames();
        }

        public void ExtractPartNames()
        {
            foreach (var part in Parts)
            {
                switch (part.Category.ToLower())
                {
                    case "Gavel":
                        Frame = part.PartName;
                        break;
                    case "Balk":
                        Beam = part.PartName;
                        break;
                    case "genomskjutningsskydd":
                        BackCover = part.PartName;
                        break;
                }
            }
        }
    }
}
