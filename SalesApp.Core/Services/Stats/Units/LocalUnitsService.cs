using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.BL.Models.Stats.Units;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Stats.Units
{
    public class LocalUnitsService
    {
        private UnitsController _unitsInfoController;
        private ILog _logger;

        public LocalUnitsService()
        {
            _logger = Resolver.Instance.Get<ILog>();;
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

        private UnitsController UnitsInfoController
        {
            get
            {
                this._unitsInfoController = this._unitsInfoController ?? new UnitsController();
                return this._unitsInfoController;
            }
        }

        public async Task<bool> SetAsync(List<DsrUnitsInfo> units)
        {
            if (units == null || units.Count == 0)
            {
                return true;
            }

            await DataAccess.Instance.Connection.RunInTransactionAsync(
            async (tran) =>
            {
                var nunits = units.Where(unit => unit.Date <= DateTime.Today.AddDays(1));
                tran.DeleteAll<DsrUnitsInfo>();
                foreach (var product in nunits)
                {
                    await this.UnitsInfoController.SaveAsync(tran, product);
                }
            });

            return true;
        }

        public async Task<List<DsrUnitsInfo>> GetAsync(string sql)
        {
            return await UnitsInfoController.GetAllAsync(sql);
        }

       /* public async Task<DateTime> GetLastUpdateTimeAsync()
        {
            List<DsrUnitsInfo> units = await GetAsync();

            if (units == null || units.Count == 0)
            {
                return default(DateTime);
            }
            return units.OrderByDescending(rank => rank.DateUpdated).First().DateUpdated;
        }*/
    }
}
