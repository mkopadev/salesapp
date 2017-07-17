using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Summary
{
    public class EarningsDataObject : CommissionDataObject
    {
        public List<CommissionItem> Earnings { get; set; }
    }
}