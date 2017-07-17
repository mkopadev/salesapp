using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Tests.Api
{
    public abstract class ApiTestsBaseClass<TApiClass,TServerResponseClass> : TestsBase
        where TApiClass : ApiBase
    {
        private TApiClass _apiClass;

        public TApiClass Api
        {
            get { return _apiClass; }
        }

        public void DefineResponse(Exception exception)
        {
            Api.MakeGetCallAsync<List<TServerResponseClass>>(Arg.Any<string>())
                .Throws(exception);
        }

        public void DefineResponse(ServerResponse<TServerResponseClass> serverResponse)
        {
            Api.MakeGetCallAsync<TServerResponseClass>(Arg.Any<string>())
                .Returns
                (
                    Task.Run
                        (
                            () => serverResponse
                        )

                );
        }

        public void DefineResponse(ServerResponse<List<TServerResponseClass>> serverResponse)
        {
            Api.MakeGetCallAsync<List<TServerResponseClass>>(Arg.Any<string>())
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

        public abstract Task MockResponse400(); //Bad Request 

        public abstract Task MockResponse401(); //Unauthorized

        public abstract Task MockResponse404(); //Not Found

        public abstract Task MockResponse500(); //Internal Server Error

        public abstract Task MockResponse503(); //Service Unavailable

        public abstract Task MockNullResponse();

        public abstract Task MockMalformedJsonResponse();

        public abstract Task MockWrongJsonResponse();


        public abstract Task MockTimeout();

    }
}