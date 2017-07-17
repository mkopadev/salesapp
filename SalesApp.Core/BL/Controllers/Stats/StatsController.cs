using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.Stats
{
    public class StatsController : SQLiteDataService<Stat>
    {
        public override async Task<SaveResponse<Stat>> SaveAsync(Stat model)
        {
            model.Date = model.Date.ToMidnight();
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            criteriaBuilder.AddDateCriterion("Date", model.Date);
            List<Stat> stats = await GetManyByCriteria(criteriaBuilder);
            Logger.Debug("Pre-existing stats to delete = '~'".GetFormated(stats.Count));
            foreach (var stat in stats)
            {
                await DeleteAsync(stat);
            }

            return await base.SaveAsync(model);
        }
    }
}