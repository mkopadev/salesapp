using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Summary
{
    public class DeductionsDataObject : CommissionDataObject
    {
        public List<CommissionItem> Deductions { get; set; }
    }
}