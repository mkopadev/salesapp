using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Stats.Ranking
{
    public class LocalRankingService<TModel> where TModel : BusinessEntityBase, new()
    {
        private ILog _logger;

        public class LocalServiceController<T> : SQLiteDataService<TModel> 
        {
        }

        private LocalServiceController<TModel> _localController;

        private SQLiteDataService<TModel> LocalController
        {
            get
            {
                this._localController = this._localController ?? new LocalServiceController<TModel>();
                return this._localController;
            }
        }

        private ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }

                return _logger;
            }
        }

        public async Task<bool> SetAsync(List<TModel> items, Period period, SalesAreaHierarchy region)
        {
            if (items == null || items.Count == 0)
            {
                return true;
            }

            List<TModel> forDeletion = await this.GetListByPeriodAndArea(period, region);

            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async tran =>
                    {
                        DataAccess.Instance.StartTransaction(tran);
                        foreach (TModel item in forDeletion)
                        {
                            await DataAccess.Instance.DeleteAsync<TModel>(item.Id);
                        }
                        foreach (var item in items)
                        {
                            await this.LocalController.SaveAsync(tran, item);
                        }
                    });
            DataAccess.Instance.CommitTransaction();
            return true;
        }

        public async Task<List<TModel>> GetListByPeriodAndArea(Period period, SalesAreaHierarchy region)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

            return await LocalController.GetManyByCriteria(
                    criteriaBuilder
                        .Add("Period", (int)period)
                        .Add("Region", (int)region));
        }
    }
}