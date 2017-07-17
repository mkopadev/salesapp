using System;
using System.Collections.Generic;
using SalesApp.Core.Framework;

namespace SalesApp.Core.BL.Models.Stats.Ranking
{
    [Preserve(AllMembers = true)]
    public class DsrRankingList
    {
        public List<DsrRankInfo> Dsrs { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
