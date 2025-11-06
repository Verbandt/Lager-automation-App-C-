using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class Part(string codeName, string partName, string belongsTo, double price, int quantity)
    {
        public string CodeName { get; set; } = codeName;
        public string PartName { get; set; } = partName;
        public string BelongsTo { get; set; } = belongsTo;
        public double Price { get; set; } = price;
        public int Quantity { get; set; } = quantity;
        public int Count { get; set; } = 0;

        public double TotalCost() => Price * Count;

        public void AddPart(int modify = 0)
        {
            Count += Quantity + modify;
        }
    }
}
