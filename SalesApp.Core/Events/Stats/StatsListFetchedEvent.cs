using System;
using System.Collections.Generic;
using SalesApp.Core.Services.Stats;

namespace SalesApp.Core.Events.Stats
{
    public class StatsListFetchedEvent : EventArgs
    {
        public List<Row> Rows { get; private set; }

        public StatsListFetchedEvent(List<Row> rows)
        {
            this.Rows = rows;
        }
    }
}