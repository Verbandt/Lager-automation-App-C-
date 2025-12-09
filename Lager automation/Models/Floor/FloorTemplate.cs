namespace Lager_automation.Models
{
    public class FloorTemplate: ITemplate
    {
        public List<string> Properties { get; set; } = new();

        public string Name { get; set; } = string.Empty;
        public int HeightLimit { get; set; }
        public int WeightLimitTonageM2 { get; set; }
        public string Criteria { get; set; } = string.Empty;

        public FloorTemplate(List<string> properties)
        {
            Properties = properties;
            ExtractProperties();
        }

        public void ExtractProperties()
        {
            HeightLimit = int.Parse(Properties[0]);           // was [1]
            WeightLimitTonageM2 = int.Parse(Properties[1]);   // was [2]
            Criteria = Properties[2];                         // was [3]
            Name = $"{HeightLimit} mm";                       // 👈 NEW NAME LOGIC
        }
    }
}
