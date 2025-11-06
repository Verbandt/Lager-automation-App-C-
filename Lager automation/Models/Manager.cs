using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Lager_automation.Models
{
    public class Manager
    {
        public Bricscad Bricscad { get; set; } = new();
        public Dictionary<string, List<Section>> Sections { get; set; } = new();
        public Section? CurrentSection { get; set; }
        public int XForNextSection { get; set; } = 15000;
        public int YForNextSection { get; set; } = 20000;

        public Dictionary<string, Dictionary<string, List<Article>>> Factories { get; set; } = new();
        public Dictionary<string, (int R, int G, int B)> FactoryColors { get; set; } = new()
        {
            { "VS", (255, 127, 159) },
            { "VV", (95, 76, 153) },
            { "Common", (61, 212, 36) }
        };
        public HashSet<string> ArticlesTriedOnCurrentSection { get; set; } = new();
        public bool CreatedANewSection { get; set; } = false;
        public bool CreateNewShelf { get; set; } = false;

        public int EmbPlaced { get; set; } = 0;
        public Dictionary<string, Dictionary<string, int>> EmbTypeCounter { get; set; } = new();

        public DataTable Dt { get; set; }

        private Dictionary<string, List<Article>> AllArticles { get; set; } = new();

        public Manager()
        {
            Dt = ImportDT();
        }

        private DataTable ImportDT()
        {
            DataTable dt = new DataTable();
            using var workbook = new XLWorkbook(@"C:\Users\KMOLLER2\OneDrive - Volvo Cars\Desktop\Johannes hatar HF\Köpart nya.xlsx");
            var ws = workbook.Worksheet("Köpartiklar");

            var headers = ws.FirstRowUsed().Cells().Select(c => c.GetString().Trim()).ToList();
            headers.ForEach(h => dt.Columns.Add(h));

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var values = new List<object>();
                foreach (var header in headers)
                {
                    var cell = row.Cell(headers.IndexOf(header) + 1);
                    values.Add(cell.Value.ToString()?.Trim() ?? "");
                }
                dt.Rows.Add(values.ToArray());
            }

            const string colStock = "Säkerthetslager + marginal";
            const string colFactory = "Ovan Artikel Fabrik";

            var clean = dt.Clone();
            var cleanRows = dt.AsEnumerable().Where(r =>
            {
                string stockStr = r[colStock]?.ToString()?.Trim() ?? "";
                string factoryStr = r[colFactory]?.ToString()?.Trim() ?? "";
                return double.TryParse(stockStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v)
                    && v != 0
                    && !string.IsNullOrWhiteSpace(factoryStr);
            }).ToList();  // ✅ forces execution immediately

            foreach (var row in cleanRows)
                clean.ImportRow(row);

            return clean;
        }

        private void CountEmbType(string embType, int embNeeded, string factory, string commonPart)
        {
            factory = commonPart == "Ja" ? "Common part" : factory;
            EmbTypeCounter[factory][embType] += embNeeded;
        }

        private void AssignArticlesBasedOnFactory()
        {
            foreach (var entry in AllArticles)
            {
                var articleList = entry.Key;
                var articles = entry.Value;
                foreach (var article in articles)
                {
                    if (!Factories.ContainsKey(article.Factory))
                        Factories[article.Factory] = new Dictionary<string, List<Article>>();

                    if (!Factories[article.Factory].ContainsKey(articleList))
                        Factories[article.Factory][articleList] = new List<Article>();

                    Factories[article.Factory][articleList].Add(article);
                }
            }
        }

        private (List<Article>, List<Article>) CreateArticles()
        {
            var articles = new List<Article>();
            foreach (DataRow row in Dt.Rows)
            {
                string articleNumber = row["ARTNR"]?.ToString().Trim() ?? "";
                string customer = row["LEVNR1"]?.ToString().Trim() ?? "";
                string embName = row["EMBTYP"]?.ToString().Trim() ?? "";
                string commonPart = row["Common part"]?.ToString().Trim() ?? "";
                string factory = row["Ovan Artikel Fabrik"]?.ToString().Trim() ?? "";

                int.TryParse(row["Längd"]?.ToString(), out int embLength);
                int.TryParse(row["Bredd"]?.ToString(), out int embWidth);
                int.TryParse(row["Höjd"]?.ToString(), out int embHeight);
                int.TryParse(row["Säkerthetslager + marginal"]?.ToString(), out int embNeeded);
                double.TryParse(row["Emb brutto vikt"]?.ToString(), out double bruttoWeight);
                int bruttoWeightInt = (int)Math.Ceiling(bruttoWeight);
                int.TryParse(row["ANTALIEMB"]?.ToString(), out int fillRate);

                bool gEmb = (embLength == 1450 || embLength == 1491);

                var article = new Article(
                    articleNumber,
                    customer,
                    embName,
                    embLength,
                    embWidth,
                    embHeight,
                    embNeeded,
                    bruttoWeightInt,
                    fillRate,
                    commonPart,
                    gEmb,
                    factory
                );
                articles.Add(article);
            }

            articles = [.. articles
                    .OrderByDescending(a => a.EmbHeight)
                    .ThenByDescending(a => a.EmbNeeded)];

            List<Article> gEmbArticles = [.. articles.Where(a => a.EmbLength is 1491 or 1450)];
            List<Article> normalArticles = [.. articles.Where(a => a.EmbLength is not (1491 or 1450))];

            return (normalArticles, gEmbArticles);

        }

        private void AddTextInRect(string articleNumber, string embName, double[] textPoint, int width)
        {
            float textHeight = width * 0.1f;

            var secondLinePoint = new double[] { textPoint[0], textPoint[1] - textHeight - 130, 0 };

            Bricscad.Model.AddText(articleNumber, textPoint, textHeight);
            Bricscad.Model.AddText(embName, secondLinePoint, textHeight);
        }

        private void SetTrueColor(dynamic entity, int r, int g, int b)
        {
            dynamic color = Bricscad.App.GetInterfaceObject("BricscadDb.AcadAcCmColor");
            color.SetRGB(r, g, b);
            entity.TrueColor = color;
        }

        private void CreateNewSection(string factory, string articleType)
        {
            string rackType;
            bool doubleFrames;
            string typeOfBeam;

            if (factory == "VS")
            {
                rackType = "medium_frame_6m";
                doubleFrames = false;
                typeOfBeam = "beam_3600";
            }
            else
            {
                rackType = "heavy_frame_5m";
                doubleFrames = true;
                typeOfBeam = "beam_1900";
            }

            (int r, int g, int b) color = FactoryColors[factory];
            CurrentSection = new Section(
                XForNextSection,
                YForNextSection,
                color,
                factory,
                articleType,
                typeOfBeam
            );

            if (!Sections.ContainsKey(factory))
                Sections[factory] = new List<Section>();
            Sections[factory].Add(CurrentSection);

            XForNextSection += CurrentSection.RackLength;
            RacksCost.AddNewSection(backCover: false, rackType, doubleFrames);
        }

        private bool AllEmbPlaced(List<Article> articles)
        {
            return articles.All(a => a.EmbNeeded == 0);
        }

        private void tryToPlaceArticlesInSection(List<Article> articles, string articleType)
        {
            string factory = articles[0].Factory;

            if (CurrentSection == null)
            {
                CreateNewSection(factory, articleType);
            }

            foreach (Article article in articles)
            {
                if (CreatedANewSection)
                {
                    CreatedANewSection = false;
                    break;
                }
                else if (article.EmbNeeded == 0)
                {   
                    ArticlesTriedOnCurrentSection.Add(article.ArticleNumber);
                    continue;
                }

                for (int i = 0; i < article.EmbNeeded; i++)
                {
                    bool result = CurrentSection!.AddArticleToSection(article, CreateNewShelf);

                    if (result)
                    {
                        ArticlesTriedOnCurrentSection = new();
                        CreateNewShelf = false;
                        article.EmbNeeded--;
                        EmbPlaced++;
                    }
                    else
                    {
                        ArticlesTriedOnCurrentSection.Add(article.ArticleNumber);
                        if (ArticlesTriedOnCurrentSection.Count == articles.Count)
                        {
                            ArticlesTriedOnCurrentSection = new();

                            if (CreateNewShelf)
                            {
                                CreateNewSection(article.Factory, articleType);
                                CreatedANewSection = true;
                            }
                            else
                            {
                                CreateNewShelf = true;
                            }
                            
                        }
                        break;
                    }
                }
            }
        }

        private void AddArticlesToRacking(List<Article> articles, string articleType)
        {
            while (!AllEmbPlaced(articles))
            {
                tryToPlaceArticlesInSection(articles, articleType);
            }

            ArticlesTriedOnCurrentSection.Clear();
            CurrentSection = null;
        }

        private void PlaceAllArticlesInRacking()
        {
            foreach (var factoryEntry in Factories)
            {
                foreach (var articleTypeEntry in factoryEntry.Value)
                {
                    string articleType = articleTypeEntry.Key;
                    List<Article> articles = articleTypeEntry.Value;
                    AddArticlesToRacking(articles, articleType);
                }

                XForNextSection = 10000;
                YForNextSection += 20000;
            }
        }

        public void DrawRackingForEachFactory()
        {
            foreach (var factoryEntry in Sections)
            {
                DrawSectionsFromFront(factoryEntry.Value, factoryEntry.Key);
                DrawSectionsFromTop(factoryEntry.Value, factoryEntry.Key);
            }
        }

        public void DrawSectionsFromFront(List<Section> sections, string factory)
        {
            void DrawSideBeams(int rackX, int rackY, int rackLength, int rackCurrentHeight, (int, int, int) rgb)
            {
                DrawRect(rackX, rackY, 90, rackCurrentHeight, rgb);
                DrawRect(rackX + rackLength, rackY, 90, rackCurrentHeight, rgb);
            }

            void DrawEmb(List<Emb> embs)
            {
                foreach (var emb in embs)
                {
                    DrawRect(emb.X, emb.Y, emb.Length, emb.Height, emb.EmbColor);
                    double centerY = emb.Y + emb.Height / 1.2;
                    dynamic textPoint = new double[] { emb.X + 20, centerY, 0};
                    AddTextInRect(emb.ArticleNumber, emb.EmbName, textPoint, emb.Length);
                }
            }

            void DrawShelves((int, int , int) rgb, List<Shelf> shelves)
            {
                foreach (Shelf shelf in shelves)
                {
                    DrawEmb(shelf.EmbAssigned);
                    if (shelf.BeamHeight != 0)
                    {
                        DrawRect(shelf.X + 90, shelf.SectionY, shelf.Length, shelf.BeamHeight, rgb);
                    }
                }
            }

            void DrawRackWeight(Section section)
            {
                dynamic textPoint = new double[] { section.X + 1000, section.Y - 1000, 0 };
                AddTextInRect($"{section.Weight:F0}kg", "", textPoint, section.RackLength);

            }

            (int, int, int) rgb = FactoryColors[factory];
            
            foreach (var section in sections)
            {
                int rackX = section.X;
                int rackY = section.Y;
                int rackLength = section.RackLength;
                int rackWidth = section.RackWidth;
                int rackCurrentHeight = section.CurrentHeight;
                DrawSideBeams(rackX, rackY, rackLength, rackCurrentHeight, rgb);
                DrawShelves(rgb, section.Shelves);
                DrawRackWeight(section);
            }
        }

        public void DrawSectionsFromTop(List<Section> sections, string factory)
        {
            int FindHighestSectionInFactory(string factory)
            {
                List<Section> sections = Sections[factory];
                int maxHeight = 0;

                foreach (var section in sections)
                {
                    if (section.CurrentHeight > maxHeight)
                    {
                        maxHeight = section.CurrentHeight;
                    }
                }
                return maxHeight;
            }

            void DrawSectionOutline(int sectionX, int sectionY, int sectionLength, int sectionWidth, (int, int, int) rgb)
            {
                DrawRect(sectionX, sectionY, sectionLength, sectionWidth, rgb);
            }

            (int, int, int) rgb = FactoryColors[factory];
            int heightForTopView = FindHighestSectionInFactory(factory) + 5000;

            foreach (var section in sections)
            {
                int sectionX = section.X;
                int sectionY = section.Y;
                int sectionLength = section.RackLength;
                int sectionWidth = section.RackWidth;
                DrawSectionOutline(sectionX, sectionY + heightForTopView, sectionLength, sectionWidth, rgb);
            }
        }

        public void DrawRect(float baseX, float baseY, float width, float height, (int, int, int) rgb)
        {
            var bl = new double[] { baseX, baseY, 0 };
            var br = new double[] { baseX + width, baseY, 0 };
            var tr = new double[] { baseX + width, baseY + height, 0 };
            var tl = new double[] { baseX, baseY + height, 0 };

            var (r,g,b) = rgb;

            var pointPairs = new[]
            {
                (bl, br),
                (br, tr),
                (tr, tl),
                (tl, bl)
            };

            foreach (var (start, end) in pointPairs)
            {
                var line = Bricscad.Model.AddLine(start, end);
                SetTrueColor(line, r, g, b);
            }
        }

        public void BeginRackingProcess()
        {
            Dt = ImportDT();

            var (normal, gEmb) =  CreateArticles();
            AllArticles = new Dictionary<string, List<Article>>
            {
                { "normal", normal },
                { "g_emb", gEmb }
            };

            AssignArticlesBasedOnFactory();

            PlaceAllArticlesInRacking();
            DrawRackingForEachFactory();
            RacksCost.ExportToExcel("ställage_kostnad.xlsx");
        }
    }
}
