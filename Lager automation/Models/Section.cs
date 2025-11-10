using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class Section
    {
        public int X { get; }
        public int Y { get; }
        private int SectionY { get; set; }
        public (int R, int G, int B) Rgb { get; }
        public string Factory { get; }
        public string ArticleType { get; }
        public string TypeOfBeam { get; }

        public int BeamHeight { get; private set; }
        public int RackLength { get; private set; }
        public int RackWidth { get; private set; }
        public int ShelfInnerLength { get; private set; }
        private Dictionary<string, int> ShelfHeights = new()
        {
            { "L1-L3", 766 },
            { "L4", 966 },
        };
        private int SpaceAboveEmb { get; set; } = 150;
        private int SpaceInbetweenEmb { get; set; } = 60;
        private Dictionary<string, int> FactoryHeightLimits = new()
        {
            { "VS", 7000 },
            { "VV", 10000 },
            { "Common", 10000 },

        };
        public List<Shelf> Shelves { get; set; } = new();
        private int HeightLimit { get; set; }
        public int CurrentHeight { get; private set; } = 0;
        public int Weight { get; set; } = 0;
        private bool OnFloor { get; set; } = true;
        private Shelf? CurrentShelf { get; set; }

        public List<Emb> EmbAssigned = new();


        public Section(int x, int y, (int R, int G, int B) rgb, string factory, string articleType, string typeOfBeam)
        {
            X = x;
            Y = y;
            SectionY = y;
            Rgb = rgb;
            Factory = factory;
            ArticleType = articleType;
            TypeOfBeam = typeOfBeam;

            SetSectionProperties();
            HeightLimit = AssignHeightLimit(factory);
        }

        private void SetSectionProperties()
        {
            (BeamHeight, ShelfInnerLength) = TypeOfBeam switch
            {
                "beam_3600" => (140, 3600),
                "beam_1900" => (100, 1900),
                _ => (0, 0)
            };

            // Derived properties
            RackLength = ShelfInnerLength + 90;
            RackWidth = ArticleType == "g_emb" ? 1470 : 1200;
        }

        private int DecideHeightForShelf(int height)
        {
            int newHeight = height switch
            {
                <= 766 => ShelfHeights["L1-L3"],
                <= 966 => ShelfHeights["L4"],
                _ => height
            };
            return newHeight + SpaceAboveEmb;
        }

        private int AssignHeightLimit(string factory)
        {
            return FactoryHeightLimits[factory];
        }

        public bool AddArticleToSection(Article article, bool createNewShelf)
        {
            if(createNewShelf && Shelves.Count > 0)
            {
                OnFloor = false;
            }
            if (article.EmbHeight > ShelfHeights["L4"])
            {
                if(!OnFloor)
                {
                    return false;
                }
            }

            if (Shelves.Count == 0 || createNewShelf)
            {
                int heightToAdd = DecideHeightForShelf(article.EmbHeight);
                int beamHeight = CurrentHeight == 0 ? 0 : BeamHeight;

                bool succefullyCreatedNewShelf = CreateNewShelf(heightToAdd + beamHeight);
                if (!succefullyCreatedNewShelf)
                {
                    return false;
                }

                CurrentHeight += heightToAdd + beamHeight;
                SectionY += heightToAdd + beamHeight;
            }

            bool placedArticle = CurrentShelf?.AddArticleToShelf(article) ?? false;
            if (!placedArticle)
            {
                return false;
            }

            if (Shelves.Count > 1)
            {
                Weight += article.BruttoWeight;
            }

            return true;
        }

        private bool CreateNewShelf(int height)
        {
            int beamHeight;

            if(CurrentHeight == 0)
            {
               beamHeight = 0;
            }
            else { beamHeight = BeamHeight; }

            if (CurrentHeight + height + beamHeight > HeightLimit)
            {
                return false;
            }

            CurrentShelf = new Shelf(ShelfInnerLength, height, beamHeight, X, SectionY, SpaceAboveEmb, SpaceInbetweenEmb, HeightLimit);
            Shelves.Add(CurrentShelf);
            RacksCost.AddShelfLevel(false, TypeOfBeam, ShelfInnerLength);

            return true;
        }

    }

}
