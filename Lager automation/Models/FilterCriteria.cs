using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public enum FilterCriteria
    {
        [Description("Fabrik")]
        Factory,

        [Description("Kund")]
        Customer,

        [Description("Stapelhöjd")]
        StackingHeight
    }
}
