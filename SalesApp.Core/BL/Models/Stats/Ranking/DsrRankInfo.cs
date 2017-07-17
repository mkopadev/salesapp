using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Stats;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.Stats.Ranking
{
    //[Preserve(AllMembers = true)]
    public class DsrRankInfo : BusinessEntityBase
    {
        public bool IsMe { get; set; }
        public string Name { get; set; }
        public int Sales { get; set; }

        public int Rank { get; set; }


        [NotNull]
        public Period Period { get; set; }

        [NotNull]
        public SalesAreaHierarchy Region { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}