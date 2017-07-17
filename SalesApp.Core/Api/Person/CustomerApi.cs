using System;
using System.Threading.Tasks;
using MvvmCross.Platform;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.RemoteServices.ErrorHandling;
using SalesApp.Core.Services.RemoteServices.ErrorHandling.UiNotificationTypes;

namespace SalesApp.Core.Api.Person
{
    public interface ICustomerApi
    {
        Task<CustomerRegistrationResponse> RegisterCustomer(Customer customer, ApiTimeoutEnum timeOut);
    }

    public class CustomerApi : ApiBase, ICustomerApi
    {
        private static readonly ILog Log = LogManager.Get(typeof(CustomerApi));

        public CustomerApi()
            : base("customers")
        {
        }

        public async Task<CustomerRegistrationResponse> RegisterCustomer(Customer customer, ApiTimeoutEnum timeOut)
        {
            // register custom error handler for 500 error, we want to show toast and continue
            var errorDescriber500 = new ErrorDescriber(
                typeof(HttpResponse500Exception),
                typeof(BackgroundNotifier),
                ErrorFilterFlags.Ignore500Family);

            ApiErrorHandler.RegisterExpectedError(this, errorDescriber500);

            Log.Verbose("Register Customer.");
            ServerResponse<CustomerRegistrationResponse> response = 
                await this.PostObjectAsync<CustomerRegistrationResponse, Customer>(customer, timeOut: timeOut);

            Log.Verbose("API call done.");
            if (response == null)
            {
                Log.Verbose("API NOT successfull");

                return new CustomerRegistrationResponse()
                {
                    Customer = null,
                    RequestId = customer.RequestId,
                    Successful = false,
                    ResponseText = null
                };
            }

            Log.Verbose("API result: " + response.IsSuccessStatus);
            Log.Verbose(response.RawResponse);

            // try getting the object from JSON
            CustomerRegistrationResponse result = null;
            try
            {
                result = response.GetObject();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            // got a proper result, return it
            if (result != null)
            {
                result.Successful = true;
                return result;
            }

            Log.Verbose("Could not parse response.");

            // 500 error occured, special case
            if (response.RequestException.GetType().IsAssignableFrom(typeof(HttpResponse500Exception)))
            {
                return new CustomerRegistrationResponse
                {
                    Customer = null,
                    RequestId = customer.RequestId,
                    Successful = false,
                    ResponseText = "HttpResponse500Exception"
                };
            }

            // if exception was thrown, return the exception as text
            if (response.RequestException != null)
            {
                return new CustomerRegistrationResponse
                {
                    Customer = null,
                    RequestId = customer.RequestId,
                    Successful = false,
                    ResponseText = response.RequestException.ToString()
                };
            }

            // unknown error returned
            return new CustomerRegistrationResponse
            {
                Customer = null,
                RequestId = customer.RequestId,
                Successful = false,
                ResponseText = "Unknown Error."
            };
        }
    }
}