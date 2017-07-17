using System;
using SalesApp.Core.Enums.Stats;

namespace SalesApp.Droid.UI.Stats.Reporting
{
    public class RankingsUpdateRequestEventArgs : EventArgs
    {
        public RankingsUpdateRequestEventArgs(Period period, SalesAreaHierarchy salesArea)
        {
            Period = period;
            SalesArea = salesArea;
        }

        public Period Period { get; private set; }

        public SalesAreaHierarchy SalesArea { get; private set; }
    }
}