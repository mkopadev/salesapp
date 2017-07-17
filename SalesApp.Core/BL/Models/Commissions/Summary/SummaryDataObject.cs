using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Summary
{
    public class SummaryDataObject
    {
        public double Balance { get; set; }

        public List<CommissionItem> Summary { get; set; }
    }
}