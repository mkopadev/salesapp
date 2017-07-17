using System;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Person;
using SalesApp.Core.Api.Product;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Events.CustomerRegistration;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Person
{
    /// <summary>
    /// This service can be used to register a Person (for now Customers only are supported).
    /// </summary>
    public class CustomerService
    {
        private static readonly ILog Log = LogManager.Get(typeof(CustomerService));

        private ICustomerApi customerApi;
        private ICustomerApiFallback customerApiFallback;

        private IAdditionalProduct additionalProductApi;

        private int numberOfRetries = 3;

        public int SmsCurrentTry { get; set; }

        public event EventHandler<CustomerRegistrationCompletedEventArgs> RegistrationCompleted;

        public event EventHandler<CustomerRegistrationAttemptedEventArgs> RegistrationAttempted;

        public bool RegistrationSuccessful { get; set; }

        public async Task AttemptSMSRegistration(BL.Models.People.Customer customer)
        {
            Log.Verbose("Customer not yet registered, try SMS.");

            await this.RegisterViaFallback(customer);
            this.SmsCurrentTry++;
        }

        private async Task<bool> AttemptDataRegistration(BL.Models.People.Customer customer)
        {
            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                return false;
            }

            // try to post the person via the API, for configurable x tries
            CustomerRegistrationResponse response = null;
            int apiTries = 1;
            while (apiTries <= this.numberOfRetries)
            {
                Log.Verbose("API register try " + apiTries);

                // call the API and await response
                response = await this.RegisterViaApi(customer);
                response.Customer = customer;

                if (response.Successful)
                {
                    this.RegistrationSuccessful = true;

                    if (response.RegistrationId != Guid.Empty)
                    {
                        var registrationCompletedEventArgs = new CustomerRegistrationCompletedEventArgs(
                            DataChannel.Full,
                            true) {RegistrationId = response.RegistrationId};

                        this.FireRegistrationCompleted(registrationCompletedEventArgs);
                    }
                    else
                    {
                        this.FireRegistrationCompleted(DataChannel.Full, true);
                    }

                    return true;
                }

                this.FireRegistrationAttempted(DataChannel.Full, apiTries);

                // exception on reported 500 error, do not continue
                if (response.ResponseText.Equals("HttpResponse500Exception"))
                {
                    Log.Error("API threw 500 error (HttpResponse500Exception).");
                    break;
                }

                // next retry
                apiTries++;
            }

            // make sure the warning logging never fails, it is extra information
            try
            {
                var responseText = response == null ? "response=null" : response.ResponseText;
                Log.Warning("Fallback Registration: Attempts to register via Internet failed. ResponseText = " +
                            responseText);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }


            return false;
        }

        private void FireRegistrationAttempted(DataChannel channel, int currentTry)
        {
            if (this.RegistrationAttempted != null)
            {
                this.RegistrationAttempted(this, new CustomerRegistrationAttemptedEventArgs(channel, currentTry, this.numberOfRetries));
            }
        }

        private void FireRegistrationCompleted(DataChannel channel, bool succeeded)
        {
            if (this.RegistrationCompleted != null)
            {
                this.RegistrationCompleted(this, new CustomerRegistrationCompletedEventArgs(channel, succeeded));
            }
        }

        private void FireRegistrationCompleted(CustomerRegistrationCompletedEventArgs e)
        {
            if (this.RegistrationCompleted != null)
            {
                this.RegistrationCompleted(this, e);
            }
        }

        public CustomerService()
        {
            this.customerApi = new CustomerApi();
            this.customerApiFallback = new CustomerApiFallback();
            this.additionalProductApi = new AdditionalProductApi();

            // registerfor SMS events
            this.customerApiFallback.SmsFailedEvent += this.SmsNotSend;
            this.customerApiFallback.SmsSentEvent += this.SmsSend;
        }

        private void SmsNotSend(object sender, EventArgs args)
        {
            int maxTries = Settings.Settings.Instance.MaxFallbackRetries;
            if (this.SmsCurrentTry >= maxTries)
            {
                this.RegistrationSuccessful = true;
                this.FireRegistrationCompleted(DataChannel.Fallback, false);
            }
            else
            {
                this.FireRegistrationAttempted(DataChannel.Fallback, this.SmsCurrentTry);
            }
        }

        private void SmsSend(object sender, EventArgs args)
        {
            this.RegistrationSuccessful = true;
            this.FireRegistrationCompleted(DataChannel.Fallback, true);
        }

        /// <summary>
        /// Attempts to register customer
        /// </summary>
        /// <param name="customer">The customer model</param>
        /// <returns>The data channel used to register the customer</returns>
        public async Task RegisterCustomer(BL.Models.People.Customer customer)
        {
            if (customer.Product != null)
            {
                customer.Product.Created = DateTime.Now;
                customer.Product.Modified = DateTime.Now;
            }

            if (!(await this.AttemptDataRegistration(customer)))
            {
                await this.AttemptSMSRegistration(customer);
            }
        }

        /// <summary>
        /// This method will try and register the customer via normal (internet) method.
        /// </summary>
        /// <param name="customer">The customer to register</param>
        /// <returns>A customer registration response object</returns>
        private Task<CustomerRegistrationResponse> RegisterViaApi(BL.Models.People.Customer customer)
        {
            try
            {
                Task<CustomerRegistrationResponse> response;

                if (customer.IsAdditionalProduct)
                {
                    response = this.additionalProductApi.RegisterAdditionalProduct(customer);
                }
                else
                {
                    var timeout = ApiTimeoutEnum.Short;
                    CountryCodes dsrCountryCode = Settings.Settings.Instance.DsrCountryCode;
                    if (dsrCountryCode == CountryCodes.UG)
                    {
                        timeout = ApiTimeoutEnum.Normal;
                    }
                    response = this.customerApi.RegisterCustomer(customer, timeout);
                }

                return response;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Task.FromResult(new CustomerRegistrationResponse
                {
                    Customer = customer,
                    RequestId = customer.RequestId,
                    Successful = false,
                    ResponseText = e.Message
                });
            }
        }

        /// <summary>
        /// This method will try and register the customer via fallback method.
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private Task<bool> RegisterViaFallback(BL.Models.People.Customer customer)
        {
            try
            {
                return this.customerApiFallback.RegisterCustomer(customer);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Task.FromResult(false);
            }
        }
    }
}