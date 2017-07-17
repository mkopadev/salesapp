using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.Api.People.Customers;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.Person.Customer;

namespace SalesApp.Core.Tests.Services.Person
{
    [TestFixture]
    public class CustomerSearchServiceTest : RemoteServicesTestsBase<CustomerSearchApi, List<CustomerSearchResult>, CustomerSearchService, CustomerSearchResult>
    {
        protected override List<CustomerSearchResult> ValidResponse
        {
            get
            {
                return new List<CustomerSearchResult>
                {
                    new CustomerSearchResult
                    {
                        Channel = DataChannel.Full,
                        DsrPhone = "0721553229",
                        FirstName = "Steve",
                        LastName = "Kariuki",
                        NationalId = "11245368",
                        Phone = "0720000000",
                        Product = new Product
                        {
                            ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb")
                            ,
                            SerialNumber = "100001"
                        },
                        RequestId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"),
                        UserId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb"),
                        Created = DateTime.Now
                    },
                    new CustomerSearchResult
                    {
                        Channel = DataChannel.Full,
                        DsrPhone = "0721553229",
                        FirstName = "John",
                        LastName = "Doe",
                        NationalId = "112453685",
                        Phone = "0720000001",
                        Product = new Product
                        {
                            ProductTypeId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429eb")
                            ,
                            SerialNumber = "100002"
                        },
                        RequestId = new Guid("812bd11a-7890-4af3-af95-ffa15b9429ee"),
                        UserId = new Guid("812bd11a-7890-4aa3-af95-ffa15b9429eb"),
                        Created = DateTime.Now
                    }
                };
            }
        }

        [Test]
        public async override Task ApiCallsOnline()
        {
            DefineResponse(ValidResponse);

            Assert.DoesNotThrow(
                    async () =>
                    {
                        List<CustomerSearchResult> results = await
                        RemoteService.GetAsync("john");

                        Assert.IsNotNull(results);
                        Assert.AreEqual(results.Count,
                            ValidResponse.Select(res => res.RequestId)
                                .Intersect(results.Select(r => r.RequestId))
                                .Count());
                    });
        }

        // [Test] Broken test
        public async override Task ApiCallsOffline()
        {
            DefineResponse(new NotConnectedToInternetException());
            Assert.DoesNotThrow
                (
                    async () =>
                    {
                        var res = await RemoteService.GetAsync("john");
                        Assert.IsNotNull(res);
                        Assert.AreEqual(0, res.Count);
                    }
                );
        }

        // [Test] Broken test
        public async override Task ApiCallsCanceled()
        {
            this.ApiCallCanceledBoilerPlate
                 (
                     Assert.IsEmpty
                 );
        }

        // [Test] Todo Test completely broken ILocationService not bound to real implementatio, cannot be resolved
        public async override Task MockCommonHttpFailureCodes()
        {
            await MockCommonHttpFailureCodesBoilerPlate(null);
        }

        public override Task MockNullResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockMalformedJsonResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockWrongJsonResponse()
        {
            throw new NotImplementedException();
        }

        public override Task MockTimeout()
        {
            throw new NotImplementedException();
        }

        // [Test] Broken test
        public async override Task ActualOnlineCall()
        {
            RemoteService.Api = new CustomerSearchApi();
            var result = await RemoteService.GetAsync("john");
            Assert.IsNotNull(result);
        }
    }
}