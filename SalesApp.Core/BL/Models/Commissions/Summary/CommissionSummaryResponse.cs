namespace SalesApp.Core.BL.Models.Commissions.Summary
{
    public class CommissionSummaryResponse
    {
        public EarningsDataObject Earnings { get; set; }

        public DeductionsDataObject Deductions { get; set; }

        public SummaryDataObject Summary { get; set; }
    }
}