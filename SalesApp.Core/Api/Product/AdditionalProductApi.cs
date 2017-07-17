using System;
using System.Threading.Tasks;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Logging;

namespace SalesApp.Core.Api.Product
{
    public class AdditionalProductApi : ApiBase, IAdditionalProduct
    {
        private static readonly ILog Log = LogManager.Get(typeof(AdditionalProductApi));

        public AdditionalProductApi() : base("additionalproduct")
        {
        }

        public async Task<CustomerRegistrationResponse> RegisterAdditionalProduct(Customer customer)
        {
            Log.Verbose("Requesting additonal product");
            ServerResponse<CustomerRegistrationResponse> responseObj = await PostObjectAsync<CustomerRegistrationResponse, Customer>(customer);
            if (responseObj == null)
            {
                Log.Verbose("API CALL NOT successfull");

                return new CustomerRegistrationResponse()
                {
                    Customer = null,
                    RequestId = customer.RequestId,
                    Successful = false,
                    RegistrationSuccessful = false,
                    ResponseText = null
                };
            }

            Log.Verbose("API Response " + responseObj.IsSuccessStatus);
            Log.Verbose(responseObj.RawResponse);

            // try getting the object from JSON
            CustomerRegistrationResponse response = null;
            try
            {
                response = responseObj.GetObject();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            // got a proper result, return it
            if (response != null)
            {
                response.Successful = true;
                return response;
            }

            Log.Verbose("Could not parse response.");

            // if exception was thrown, return the exception as text
            if (responseObj.RequestException != null)
            {
                return new CustomerRegistrationResponse()
                {
                    Customer = null,
                    RequestId = customer.RequestId,
                    Successful = false,
                    RegistrationSuccessful = false,
                    ResponseText = responseObj.RequestException.ToString()
                };
            }

            return new CustomerRegistrationResponse()
            {
                Customer = null,
                RequestId = customer.RequestId,
                Successful = false,
                RegistrationSuccessful = false,
                ResponseText = "Unknown Error."
            };
        }
    }

    public interface IAdditionalProduct
    {
        Task<CustomerRegistrationResponse> RegisterAdditionalProduct(Customer customer);
    }
}