using System;
using SalesApp.Core.BL.Models.Stats.Reporting;

namespace SalesApp.Core.Events.Stats.Reporting
{
    public class ReportingStatsDataFetchedEvent : EventArgs
    {
        public ReportingLevelEntity Entity { get; private set; }

        public ReportingStatsDataFetchedEvent(ReportingLevelEntity entity)
        {
            this.Entity = entity;
        }
    }
}