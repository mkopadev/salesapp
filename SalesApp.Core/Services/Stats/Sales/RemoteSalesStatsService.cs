using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.Stats.Sales
{
    public class RemoteSalesStatsService : RemoteServiceBase<SalesStatsApi, Stat, StatHeader>
    {
        private SalesStatsApi SalesStatsApi
        {
            get
            {
                this.Api = this.Api ?? new SalesStatsApi();
                return this.Api;
            }
        }

        public async Task<List<AggregatedStat>> UpdateStats(Period period)
        {
            try
            {
                DateTime from =
                    DateTime.Now.AddMonths(-1 * Settings.Settings.Instance.IndividualStatsLookBackMonths);
                DateTime to = DateTime.Now;

                var serverResponse = await this.SalesStatsApi.FetchStats(@from, to);
                if (serverResponse == null)
                {
                    return new List<AggregatedStat>();
                }

                StatHeader header = serverResponse.GetObject();
                StatsController statsController = new StatsController();

                for (int i = 0; i < header.Stats.Count; i++)
                {
                    header.Stats[i].Header = header;

                    await statsController.SaveAsync(header.Stats[i]);
                }

                return await new LocalSalesStatsService().GetAggregatedStats(period);
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.HandleException(jsonReaderException, "Attempt to parse invalid JSON may have been made.");
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.HandleException(taskCanceled, "Timeout may have occured or task may have been explicitly canceled by user.");
            }
            catch (Exception exception)
            {
                if (!this.HandleException(exception, string.Empty))
                {
                    throw;
                }
            }

            return new List<AggregatedStat>();
        }
    }
}
