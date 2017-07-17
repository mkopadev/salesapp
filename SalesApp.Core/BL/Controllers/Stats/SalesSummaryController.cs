using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Reporting;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.Stats
{
    public class SalesSummaryController : SQLiteDataService<SalesSummary>
    {
        public override async Task<SaveResponse<SalesSummary>> SaveAsync(SalesSummary model)
        {
            return await base.SaveAsync(model);
        }
    }
}