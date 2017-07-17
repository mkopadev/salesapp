using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Tests.Services
{
    public abstract class RemoteServicesTestsBase<TApiClass, TServerResponse, TRemoteService, TModel> : TestsBase
        where TApiClass : ApiBase
        where TModel : BusinessEntityBase, new()
        where TRemoteService : RemoteServiceBase<TApiClass, TModel, TServerResponse>, new()
    {
        private TApiClass _apiClass;

        protected abstract TServerResponse ValidResponse { get; }

        public TApiClass Api
        {
            get { return _apiClass; }
        }

        public HttpStatusCode[] CommonFailureCodes
        {
            get
            {
                return new HttpStatusCode[]
                {
                    HttpStatusCode.BadRequest
                    , HttpStatusCode.Unauthorized
                    , HttpStatusCode.NotFound
                    , HttpStatusCode.InternalServerError
                    , HttpStatusCode.ServiceUnavailable
                };
            }
        }


        protected TRemoteService RemoteService
        {
            get
            {
                return new TRemoteService {Api = Api};
            }
        }



    public void DefineResponse(Exception exception)
        {
            Api.MakeGetCallAsync<TServerResponse>(Arg.Any<string>())
                .Throws(exception);
        }

        public void DefineResponse(CancellationToken cancellationToken)
        {
            Api.MakeGetCallAsync<TServerResponse>(Arg.Any<string>())
                 .Returns
                 (
                     Task.Run
                     (
                         () =>
                         {
                             for (int i = 0; i < 29; i++)
                             {
                                 if (!cancellationToken.IsCancellationRequested)
                                 {
                                     Logger.Debug("Sleep Number: ~".GetFormated(i + 1));
                                 }
                                 else
                                 {
                                     Logger.Debug("Cancellation requested... No more sleep.");
                                     break;
                                 }
                                 Thread.Sleep(1000);
                             }
                             return default(ServerResponse<TServerResponse>);
                         }
                         , cancellationToken
                     )
                 );
        }

        public void DefineResponse(TServerResponse serverResponse)
        {
            DefineResponse
                (
                    new ServerResponse<TServerResponse>
                    {
                        IsSuccessStatus = true
                        ,
                        RawResponse = JsonConvert.SerializeObject(serverResponse)
                        ,
                        StatusCode = HttpStatusCode.OK
                        ,
                        RequestException = null
                    }
                );
        }

        public void DefineResponse(ServerResponse<TServerResponse> serverResponse)
        {
            Api.MakeGetCallAsync<TServerResponse>(Arg.Any<string>())
                .Returns
                (
                    Task.Run
                        (
                            () => serverResponse
                        )

                );
        }

        [SetUp]
        public override void Setup()
        {
            try
            {
                base.Setup();
                _apiClass = Substitute.ForPartsOf<TApiClass>();
            }
            catch (Exception exception)
            {
                "Error occured ~".GetFormated(exception.Message);
                "Stacktrace ~".GetFormated(exception.StackTrace);
                throw;
            }
        }

        public abstract Task ApiCallsOnline();

        public abstract Task ApiCallsOffline();

        public abstract Task ApiCallsCanceled();

        protected void ApiCallCanceledBoilerPlate(Action<TServerResponse> assertions)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource(5000);
            DefineResponse(cancellationToken.Token);
            Assert.DoesNotThrow
                (
                    async () =>
                    {
                        TServerResponse response = await RemoteService.GetAsync("john");
                        if (assertions != null)
                        {
                            assertions(response);
                        }
                    }
                );
        }

        public abstract Task MockCommonHttpFailureCodes();

        public abstract Task MockNullResponse();

        public abstract Task MockMalformedJsonResponse();

        public abstract Task MockWrongJsonResponse();


        public abstract Task MockTimeout();

        public abstract Task ActualOnlineCall();


        protected async Task MockCommonHttpFailureCodesBoilerPlate(Action<TServerResponse> assertions)
        {
            HttpStatusCode currentCode = HttpStatusCode.OK;
            Assert.DoesNotThrow
                (
                    async () =>
                    {
                        foreach (var failureCode in CommonFailureCodes)
                        {
                            currentCode = failureCode;
                            Logger.Debug(string.Format("Testing httpstatus '{0}'", failureCode.ToString()));
                            DefineResponse
                                (
                                    new ServerResponse<TServerResponse>
                                    {
                                        IsSuccessStatus = false
                                        ,
                                        RawResponse = ""
                                        ,
                                        StatusCode = failureCode
                                    }
                                );
                            Logger.Debug("Validating result for code " + currentCode.ToString());
                            var rawResponse = await RemoteService.GetRawServerReponseAsync("john", ErrorFilterFlags.DisableErrorHandling);
                            Assert.IsNotNull(rawResponse);
                            Assert.IsTrue(currentCode == rawResponse.StatusCode);
                            if (assertions != null)
                            {
                                assertions(rawResponse.GetObject());
                            }
                        }
                    });
        }
    }
}