using System;
using SalesApp.Core.Enums.Stats;

namespace SalesApp.Core.BL.Models.Stats.Sales
{
    public class AggregatedStat
    {
        public Period Period { get; set; }
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public int Sales { get; set; }

        public int Prospects { get; set; }
    }
}