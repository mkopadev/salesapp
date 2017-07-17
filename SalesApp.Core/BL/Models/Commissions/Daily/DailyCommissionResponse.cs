using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Daily
{
    public class DailyCommissionResponse
    {
        public string Info { get; set; }

        public double Total { get; set; }

        public List<DailyCommissionItem> DailyCommission { get; set; }
    }
}