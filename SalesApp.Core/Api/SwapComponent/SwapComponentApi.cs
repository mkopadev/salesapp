using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SalesApp.Core.Enums;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Logging;

namespace SalesApp.Core.Api.SwapComponent
{
    public class SwapComponentApi : ApiBase
    {
        private static readonly ILog Log = LogManager.Get(typeof(SwapComponentApi));

        public SwapComponentApi()
            : base("v2/swap")
        {
        }

        public async Task<CustomerDetailsResponse> GetCustomerDetails(string customerParams, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling)
        {
            Log.Verbose("Get details of a customer.");
            ServerResponse<CustomerDetailsResponse> response = null;
            try
            {
                response = await MakeGetCallAsync<CustomerDetailsResponse>(customerParams, filterFlags: filterFlags);
            }
            catch (Exception e)
            {
                Log.Verbose("Error during API call.");
                Log.Error(e);
                return new CustomerDetailsResponse()
                {
                    Successful = false,
                    Status = ServiceReturnStatus.NoInternet,
                };
            }

            Log.Verbose("API call done.");
            if (response == null || response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.RawResponse))
            {
                Log.Verbose("API call not successfull");
                return new CustomerDetailsResponse()
                {
                    Successful = false,
                    Status = ServiceReturnStatus.NoInternet,
                };
            }

            Log.Verbose(response.RawResponse);

            // try getting the object from JSON
            CustomerDetailsResponse result = null;
            try
            {
                result = response.GetObject();
            }
            catch (Exception e)
            {
                Log.Verbose("Error try getting the object from JSON.");
                Log.Error(e);
                return new CustomerDetailsResponse()
                {
                    Successful = false,
                    Status = ServiceReturnStatus.ParseError
                };
            }

            // got a proper result, return it
            if (result != null)
            {
                result.Successful = true;
                if (result.CustomerFound)
                {
                    if (result.Surname != null)
                    {
                        result.Surname = result.Surname.Trim();
                    }

                    if (result.OtherNames != null)
                    {
                        result.OtherNames = result.OtherNames.Trim();
                    }
                }

                return result;
            }

            return new CustomerDetailsResponse()
            {
                Successful = false,
                CustomerFound = false
            };
        }

        public async Task<ProductComponentsResponse> GetProductDetails(string productParams, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling)
        {
            Log.Verbose("Get components of a product.");
            ServerResponse<List<ProductComponent>> response = null;
            try
            {
                response = await MakeGetCallAsync<List<ProductComponent>>(productParams, filterFlags: filterFlags);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            Log.Verbose("API call done.");
            if (response == null)
            {
                Log.Verbose("API NOT successfull");

                return new ProductComponentsResponse()
                {
                    Successful = false,
                    ResponseText = "not_connected",
                };
            }

            Log.Verbose(response.RawResponse);

            // try getting the object from JSON
            List<ProductComponent> components = null;
            ProductComponentsResponse result = null;
            try
            {
                components = response.GetObject();
                result = new ProductComponentsResponse
                {
                    ProductComponents = components,
                };
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

            return new ProductComponentsResponse()
            {
                Successful = false,
            };
        }

        public async Task<SwapComponentResponse> SwapComponent(SwapComponentRequest request, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeout = ApiTimeoutEnum.Long)
        {
            Log.Verbose("Swapping component.");
            ServerResponse<SwapComponentResponse> response = null;
            try
            {
                response = await PostObjectAsync<SwapComponentResponse, SwapComponentRequest>(request, filterFlags: filterFlags, timeOut: timeout);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new SwapComponentResponse()
                {
                    Success = false,
                    Status = ServiceReturnStatus.NoInternet,
                    Message = "A connection error has occurred. Check your internet settings."
                };
            }

            Log.Verbose("API call done.");

            if (response == null)
            {
                Log.Verbose("API NOT successfull");
                return new SwapComponentResponse()
                {
                    Success = false,
                    Status = ServiceReturnStatus.NoInternet,
                    Message = "A connection error has occurred. Check your internet settings."
                };
            }

            Log.Verbose("API result:");
            Log.Verbose(response.RawResponse);

            // try getting the object from JSON
            SwapComponentResponse result = null;
            try
            {
                result = response.GetObject();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new SwapComponentResponse()
                {
                    Success = false,
                    Status = ServiceReturnStatus.ParseError,
                    Message = "An error has occurred when getting the server response."
                };
            }

            // got a proper result, return it
            if (result != null)
            {
                return result;
            }

            Log.Verbose("Could not parse response.");

            return new SwapComponentResponse()
            {
                Success = false,
                Status = ServiceReturnStatus.ParseError,
                Message = "An error has occurred when getting the server response."
            };
        }
    }
}
