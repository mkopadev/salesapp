using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Framework;

namespace SalesApp.Core.BL.Models.Stats.Ranking
{
    [Preserve(AllMembers = true)]
    public class RankingSummarized : BusinessEntityBase
    {
        public string Area { get; set; }

        public int DsrRank { get; set; }

        public int TotalDsrs { get; set; }

        public int Level { get; set; }
    }
}