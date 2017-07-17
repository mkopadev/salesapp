using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Stats.Reporting
{
    public class SalesSummary : BusinessEntityBase
    {

            public int Today { get; set; }
            public int ThisWeek { get; set; }
            public int Month { get; set; }
        
    }
}
