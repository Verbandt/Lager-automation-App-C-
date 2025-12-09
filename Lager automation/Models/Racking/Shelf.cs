using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class Shelf(int length, int height, int beamHeight, int x, int y, int spaceAboveEmb, int spaceInbetweenEmb, int heightLimit)
    {
        public HashSet<string> ArticleNumbersAssigned { get; set; } = new();
        public List<Emb> EmbAssigned { get; private set; } = new();
        
        public int Length { get; set; } = length;
        private int LengthLeft { get; set; } = length - spaceInbetweenEmb;
        public int Height { get; set; } = height;
        public int BeamHeight { get; set; } = beamHeight;
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int SpaceAboveEmb { get; set; } = spaceAboveEmb;
        public int SpaceInbetweenEmb { get; set; } = spaceInbetweenEmb;
        public int HeightLimit { get; set; } = heightLimit;

        public int SectionX { get; set; } = x + 90 + spaceInbetweenEmb;
        public int SectionY { get; set; } = y;
        private int EmbX { get; set; } = x + 90 + spaceInbetweenEmb;
        private int EmbY { get; set; } = y + beamHeight;

        public bool AddArticleToShelf(Article article)
        {
            string articleNumber = article.ArticleNumber;
            string embName = article.EmbName;
            int embWidth = article.EmbWidth;
            int embHeight = article.EmbHeight;
            string embType = article.EmbType;

            bool embCanFit = EmbCanFit(embWidth, embHeight);
            if (!embCanFit)
                return false;

            ArticleNumbersAssigned.Add(articleNumber);
            EmbAssigned.Add(new Emb(EmbX, EmbY, embWidth, embHeight, articleNumber, embName, embType));

            int lengthToAdd = embWidth + SpaceInbetweenEmb;
            LengthLeft -= lengthToAdd;
            EmbX += lengthToAdd;

            return true;
        }

        private bool EmbCanFit(int embLength, int embHeight)
        {
            if (embLength + SpaceInbetweenEmb > LengthLeft)
                return false;

            if (embHeight + SpaceAboveEmb + BeamHeight > HeightLimit)
                return false;

            if (embHeight + SpaceAboveEmb > Height)
                return false;

            return true;
        }
    }
}
