using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Stats.Units;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.Stats.Units
{
    public class RemoteUnitsService : RemoteServiceBase<UnitsStatsApi, DsrUnitsInfo, List<DsrUnitsInfo>>
    {
        private LocalUnitsService _unitsStatsService;

        private UnitsStatsApi UnitsApi
        {
            get
            {
                this.Api = this.Api ?? new UnitsStatsApi();
                return this.Api;
            }
        }

        private LocalUnitsService LocalUnitsService
        {
            get
            {
                this._unitsStatsService = this._unitsStatsService ?? new LocalUnitsService();
                return this._unitsStatsService;
            }
        }

        public async Task<List<DsrUnitsInfo>> UpdateUnitsHistory()
        {
            try
            {
                string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();
                var serverResponse = await this.UnitsApi.MakeGetCallAsync<List<DsrUnitsInfo>>(userId, filterFlags: ErrorFilterFlags.AllowEmptyResponses | ErrorFilterFlags.IgnoreNoInternetError);

                if (serverResponse != null && serverResponse.IsSuccessStatus)
                {
                    List<DsrUnitsInfo> units = serverResponse.GetObject();
                    await this.LocalUnitsService.SetAsync(units);

                    return units.OrderByDescending(x => x.Date).ToList();
                }
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.Logger.Error(jsonReaderException);
            }
            catch (NotConnectedToInternetException nctiex)
            {
                this.Logger.Error(nctiex);
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.Logger.Error(taskCanceled);
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
            }

            return new List<DsrUnitsInfo>();
        }
    }
}
