using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mkopa.Core.BL.Contracts;

namespace Mkopa.Core.BL.Models.Stats.Ranking
{
    class DsrUnitsInfo : BusinessEntityBase
    {
        public int Sales { get; set; }

        public DateTime SalesDate { get; set; }
    }
}
