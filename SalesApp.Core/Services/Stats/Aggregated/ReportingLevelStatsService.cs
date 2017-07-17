using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.Api.Stats;
using SalesApp.Core.BL.Cache;
using SalesApp.Core.BL.Models.Stats.Reporting;
using SalesApp.Core.Enums;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Stats.Aggregated
{
    /// <summary>
    /// This service allows for retrieving reporting level stats.
    /// </summary>
    public class ReportingLevelStatsService
    {
        private const string SelectionCacheToken = "SelectionCacheToken";

        private static readonly ILog Logger = LogManager.Get(typeof(ReportingLevelStatsService));

        private AggregatedReportStatsApi api = new AggregatedReportStatsApi();
        private IConnectivityService connectivityService = Resolver.Instance.Get<IConnectivityService>();

        /// <summary>
        /// This method retrieves the ReportingLevel stats for a user, given a level and period
        /// </summary>
        /// <param name="selection">Selection of parameters</param>
        /// <param name="ignoreCache">If true, ignores any cached information (forced remote)</param>
        /// <returns>The reporting level stats for the give level and user</returns>
        public async Task<ReportingLevelEntity> RetrieveStats(SelectionCache selection, bool ignoreCache = false)
        {
            var cacheTokenItemId = selection.SelectedItemId;
            if (selection.SelectedLevel == SelectionCache.CountryLevel)
            {
                cacheTokenItemId = "country-level";
            }

            var cacheToken = string.Format(
                "{0}-{1}-{2}",
                cacheTokenItemId,
                selection.SelectedPeriodType,
                selection.SelectedLevel);

            // before using API, check the cache
            if (!ignoreCache)
            {
                var cachedResult = await MemoryCache.Instance.Get<ReportingLevelEntity>(cacheToken);

                // valid cache found, return that
                if (cachedResult != null)
                {
                    Logger.Debug("Return cached result.");
                    return cachedResult;
                }
            }

            // if no connection do not try to continue with the API
            if (!this.connectivityService.HasConnection())
            {
                Logger.Error("No connection, unable to get stats.");
                return new ReportingLevelEntity { Status = ServiceReturnStatus.NoInternet };
            }

            Logger.Debug("No valid cache, get new stats.");
            var response = await this.api.FetchStats(selection.SelectedLevel, selection.SelectedItemId, selection.SelectedPeriodType);

            // if we have no response, return default
            if (response == null)
            {
                Logger.Error("Response == null, Server Error.");
                return new ReportingLevelEntity { Status = ServiceReturnStatus.ServerError };
            }

            // if the response was is not succesful, return default
            if (!response.IsSuccessStatus)
            {
                Logger.Error("API reports No Success, Server Error.");
                return new ReportingLevelEntity { Status = ServiceReturnStatus.ServerError };
            }

            // get the actual object from the response
            var result = response.GetObject();

            // if we do not have a response, return default object
            if (result == null)
            {
                Logger.Error("Unable to parse content from server, Parse Error.");
                return new ReportingLevelEntity { Status = ServiceReturnStatus.ParseError };
            }

            // seems all is ok
            result.Status = ServiceReturnStatus.Success;

            if (result.HasValidData)
            {
                await MemoryCache.Instance.Store(cacheToken, result);
            }

            // return the object, it is valid
            return result;
        }

        /// <summary>
        /// Retrieves the current cached user selection for the Reporting Level stats.
        /// </summary>
        /// <returns>SelectionCache from cache if existing, otherwise default SelectionCache</returns>
        public async Task<SelectionCache> RetrieveSelection()
        {
            var cachedSelection = await MemoryCache.Instance.Get<SelectionCache>(SelectionCacheToken) ?? new SelectionCache();

            return cachedSelection;
        }

        /// <summary>
        /// Stores the current selection of Reporting Level stats screen for the user.
        /// </summary>
        /// <param name="selectionCache">SelectionCache to store</param>
        public void StoreSelection(SelectionCache selectionCache)
        {
            if (selectionCache != null)
            {
               MemoryCache.Instance.Store(SelectionCacheToken, selectionCache);
            }
        }

        public ReportStat[] Filter(ReportStat[] list, SelectionCache selectionCache)
        {
            return list.Where(r => r.Date == selectionCache.SelectedPeriod).ToArray();
        }

        /// <summary>
        /// Selects the next period from the ReportStat array, given the current period.
        /// </summary>
        /// <param name="list">List to check</param>
        /// <param name="current">Current period</param>
        /// <returns>Next period if any, otherwise null</returns>
        public string NextPeriod(ReportStat[] list, string current)
        {
            // get distinct items
            string[] periods = list.Select(r => r.Date).Distinct().ToArray();
            return periods.SelectNextItem(current);
        }

        public string PreviousPeriod(ReportStat[] list, string current)
        {
            string[] periods = list.Select(r => r.Date).Distinct().ToArray();
            return periods.SelectPreviousItem(current);
        }

        /// <summary>
        /// Selects the first period from the ReportStat list.
        /// </summary>
        /// <param name="list">List to select from</param>
        /// <returns>First date item</returns>
        public string FirstPeriod(ReportStat[] list)
        {
            // return first or null if no items
            return list.Length > 0 ? list[0].Date : null;
        }
    }
}
