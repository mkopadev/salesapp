using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Product
{
    public class LocalProductService
    {
        private ProductsController _productsController;
        private ILog _logger;

        private ProductsController ProductsController
        {
            get
            {
                _productsController = _productsController ?? new ProductsController();
                return _productsController;
            }
        }

        public async Task<string> GetUnitsAllocatedNowAsync()
        {
            List<BL.Product> dsrUnits = await ProductsController.GetAllAsync();
            return dsrUnits.Where(rec => rec.SerialNumber != null).ToList().Count.ToString();
        }

        public async Task<List<BL.Product>> GetAsync()
        {
           return await ProductsController.GetAllAsync();
        }

        public async Task<bool> SetAsync(List<BL.Product> productList)
        {
            if (productList == null || productList.Count == 0)
            {
                return true;
            }

            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async (tran) =>
                    {
                        tran.DeleteAll<BL.Product>();
                        foreach (var product in productList)
                        {
                            await ProductsController.SaveAsync(product);
                        }
                    });

            return true;
        }

        public async Task<DateTime> GetLastUpdateTimeAsync()
        {
            List<BL.Product> products = await GetAsync();

            if (products == null || products.Count == 0)
            {
                return default(DateTime);
            }

            return products.OrderByDescending(rank => rank.Modified).First().Modified;
        }

        protected ILog Logger
        {
            get
            {
                if (this._logger == null)
                {
                    this._logger = Resolver.Instance.Get<ILog>();
                    this._logger.Initialize(this.GetType().FullName);
                }

                return this._logger;
            }
        }

        public async Task<List<BL.Product>> GetAsync(string sql)
        {
            List<BL.Product> products = await new QueryRunner().RunQuery<BL.Product>(sql);
            return products;
        }
    }
}
