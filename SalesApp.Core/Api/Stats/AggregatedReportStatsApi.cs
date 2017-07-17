using System;
using System.Net;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Reporting;
using SalesApp.Core.Enums;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Api.Stats
{
    public class AggregatedReportStatsApi : ApiBase
    {
        public AggregatedReportStatsApi() : base("AggregatedReport/")
        {
        }

        public async Task<ServerResponse<ReportingLevelEntity>> FetchStats(int level, string userId, Period periodType)
        {
            ServerResponse<ReportingLevelEntity> serverResponse;
            try
            {
                serverResponse = await MakeGetCallAsync<ReportingLevelEntity>(
                    string.Format(
                        "{0}?level={1}&periodType={2}",
                        userId,
                        level,
                        (int)periodType));
            }
            catch (TimeoutException timeoutException)
            {
                this.Logger.Error(timeoutException);
                return new ServerResponse<ReportingLevelEntity>
                {
                    IsSuccessStatus = false,
                    RawResponse = string.Empty,
                    RequestException = timeoutException,
                    StatusCode = HttpStatusCode.GatewayTimeout,
                    Status = ServiceReturnStatus.ServerError
                };
            }
            catch (NotConnectedToInternetException exception)
            {
                this.Logger.Error(exception);
                return new ServerResponse<ReportingLevelEntity>()
                {
                    IsSuccessStatus = false,
                    RawResponse = string.Empty,
                    RequestException = exception,
                    StatusCode = HttpStatusCode.GatewayTimeout,
                    Status = ServiceReturnStatus.ServerError
                };
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                return new ServerResponse<ReportingLevelEntity>()
                {
                    IsSuccessStatus = false,
                    RawResponse = string.Empty,
                    RequestException = exception,
                    Status = ServiceReturnStatus.ServerError
                };
            }

            this.Logger.Debug(
                "Server status code is {0}".Formatted(serverResponse == null
                    ? "Response is null so no code"
                    : serverResponse.StatusCode.ToString()));

            if (serverResponse == null || serverResponse.IsSuccessStatus == false)
            {
                this.Logger.Debug("Returning false as response from server is null");
                return new ServerResponse<ReportingLevelEntity>()
                {
                    IsSuccessStatus = false,
                    RawResponse = string.Empty,
                    RequestException = new Exception("Returning false as response from server is null"),
                    Status = ServiceReturnStatus.ServerError
                };
            }

            this.Logger.Debug("Raw text is {0}".Formatted(serverResponse.RawResponse));

            return serverResponse;
        }
    }
}
