using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SQLite.Net;

namespace SalesApp.Core.BL.Controllers.Stats
{
    public class StatHeaderController : SQLiteDataService<StatHeader>
    {
        public override async Task<SaveResponse<StatHeader>> SaveAsync(SQLiteConnection tran, StatHeader model)
        {
            Logger.Debug("Saving stat header");
            await DeleteDuplicates(tran, model);
            return await base.SaveAsync(tran, model);
        }

        private async Task DeleteDuplicates(SQLiteConnection tran, StatHeader model)
            {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
                StatHeader header = await GetSingleByCriteria(
                            criteriaBuilder
                            .Add("Period", model.Period)
                            .AddDateCriterion("From", model.From)
                            .AddDateCriterion("To", model.To));

                if (header != null)
                {
                    Logger.Debug("Deleting duplicate records");
                    await new StatsController().DeleteWithCriteriaAsync(tran, new[]
                            {
                                new Criterion("StatHeaderId", header.Id)
                            }
                    );

                    await DeleteWithCriteriaAsync(tran, new[]
                            {
                                new Criterion("Id", header.Id)
                            });
                }
                else
                {
                    Logger.Debug("No duplicate records to delete");
                }
            }
        }
}