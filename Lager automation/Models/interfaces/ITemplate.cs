using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public interface ITemplate
    {
        string Name { get; set; }
        public string Criteria { get; set; }
    }
}
