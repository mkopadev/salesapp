using System;
using System.Collections.Generic;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Stats.Units
{

    public class DsrUnitsList : BusinessEntityBase
    {
        public List<DsrUnitsInfo> Units { get; set; }

        public DateTime TimeStamp { get; set; }

    }
}
