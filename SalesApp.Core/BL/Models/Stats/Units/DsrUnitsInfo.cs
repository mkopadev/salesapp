using System;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Stats.Units
{
    //[Preserve(AllMembers = true)]
    public class DsrUnitsInfo : BusinessEntityBase
    {
        public DateTime Date { get; set; }
        public int  NewAcquired { get; set; }
        public int StartedWith { get; set; }
        public int Removed { get; set; }
    }
}
