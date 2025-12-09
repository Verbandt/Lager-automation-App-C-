using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lager_automation.Models
{
    public class FailedSectionLength : Exception { }
    public class FailedSectionHeight : Exception { }
    public class FailedHeightLimit : Exception { }
    public class FailedBiggerThanL4OnShelf : Exception { }
}
