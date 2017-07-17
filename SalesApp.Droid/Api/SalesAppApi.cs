using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Logging;
using SalesApp.Core.Services;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Droid.Api
{
    /// <summary>
    /// Client for SalesApp API.
    /// </summary>
    public class SalesAppApi : ISalesAppApi
    {
        private static readonly ILog Log = LogManager.Get(typeof (SalesAppApi));
        private string baseUrl;
        public const string ApiAuthHeaderName = "Authorization";
		private HttpClient client;

        private readonly IConfigService _configService = Resolver.Instance.Get<IConfigService>();

		//private static HttpClient client;

        public SalesAppApi()
        {

            
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseProxy = true,
                Proxy = WebRequest.GetSystemWebProxy() // Should not need this for ModernHttpClient. Default client ignores system proxy.
            };

            // create client with handler (to enforce proxy use, if exists)
			if (client == null)
				client = new HttpClient(handler);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ApiAuthHeaderName, Resolver.Instance.Get<ISalesAppSession>().UserHash);

            client.Timeout = Settings.Instance.DefaultApiTimeout;
            // Public key pinning for SSL connection. Not used yet.
            //ServicePointManager.ServerCertificateValidationCallback += PinPublicKey;

            InitializeBaseUrl();
        }

        private void InitializeBaseUrl()
        {
            baseUrl = _configService.ApiUrl;
        }
        /// <summary>
        /// Pins the public key of SSL server.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslPolicyErrors">The SSL policy errors.</param>
        /// <returns></returns>
        public static bool PinPublicKey(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null || chain == null)
                return false;

            if (sslPolicyErrors != SslPolicyErrors.None)
                return false;

            // Verify against known public key within the certificate
            String pk = certificate.GetPublicKeyString();
            //if (pk.Equals(PUB_KEY))
               // return true;

            return false;
        }
        
            [Obsolete("Use CORE!!!")]
        public async Task<Person> CheckPerson(string phone)
        {
            
            string url = string.Format("{0}{1}/{2}", baseUrl, "persons", phone);
            Log.Verbose("CheckPerson: " + url);

            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Person>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<StatusDto> RegisterProspect(ProspectDto prospect)
        {
            string url = baseUrl + "prospects";
            Log.Verbose("RegisterProspect: " + url);

            var request = prospect;

            string json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await client.PostAsync(url, content);

            StatusDto statusDto = null;
            if (response.IsSuccessStatusCode)
            {
                statusDto = JsonConvert.DeserializeObject<StatusDto>(await response.Content.ReadAsStringAsync());
            }

            return statusDto;
        }

        public async Task<StatusDto> RegisterCustomer(CustomerDto customer)
        {
            string url = baseUrl + "customers";
            Log.Verbose("RegisterCustomer: " + url);

            var request = customer;

            string json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            response = await client.PostAsync(url, content);

            StatusDto statusDto = null;
            if (response.IsSuccessStatusCode)
            {
                statusDto = JsonConvert.DeserializeObject<StatusDto>(await response.Content.ReadAsStringAsync());
            }

            return statusDto;
        }

        public async Task<List<ProductDto>> GetProducts(string dsrPhone)
        {

            string url = string.Format("{0}{1}?dsrPhone={2}", baseUrl, "products", dsrPhone);
            Log.Verbose("GetProducts: " + url);
            
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync(url);
                Log.Verbose("Products retrieved");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            // if there was no response (or exception thrown), return null
            if (response == null)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<ProductDto>>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<List<MessageDto>> GetMessages(DateTime sinceDate)
        {
            //return new List<MessageDto>();

            var since = sinceDate.ToString("s"); // ISO 8016. Example 2009-06-15T13:45:30
            var url = string.Format("{0}{1}?since={2}&lang={3}", baseUrl, "messages", since, "EN");

            Log.Verbose("GetMessages: " + url);

            HttpResponseMessage response = null;

            
            try
            {
                response = await client.GetAsync(url);
                //response.EnsureSuccessStatusCode();

                string data = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<MessageDto>>(data);
            }
            catch (Exception ex)
            {
                //TODO log this exception and propagate to user
                return new List<MessageDto>();
            }
        }

        public async Task<CustomerRegistrationStatusDto> GetRegistrationSteps(string phoneNumber)
        {
            var url = string.Format("{0}{1}/Get?phoneNumber={2}", baseUrl, "customerstatus", phoneNumber);
            Log.Verbose("GetRegistrationSteps: " + url);

            HttpResponseMessage response = null;

            response = await client.GetAsync(url);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            string data = await response.Content.ReadAsStringAsync();

            if (data.Contains("CustomerNotFound"))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<CustomerRegistrationStatusDto>(data);
            }
        }
    }
}