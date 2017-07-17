using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api.Attributes;
using SalesApp.Core.Api.ServerResponseObjects;
using SalesApp.Core.Auth;
using SalesApp.Core.Enums;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Services.RemoteServices.ErrorHandling;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Core.Api
{
    /// <summary>
    /// Base class with common functionality for handling API calls
    /// </summary>
    public abstract class ApiBase
    {
        /// <summary>
        /// The authorization header
        /// </summary>
        protected const string AuthorizationHeader = "Authorization";

        /// <summary>
        /// The base URL for API calls
        /// </summary>
        private string _baseUrl;

        /// <summary>
        /// The default HTTP handler if non is provided
        /// </summary>
        private HttpClientHandler _defaultHttpHandler;

        /// <summary>
        /// The extra header to for the request
        /// </summary>
        private Dictionary<string, string> _extraHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Gets the configuration service
        /// </summary>
        private IConfigService _configService = Resolver.Instance.Get<IConfigService>();

        /// <summary>
        /// Gets an instance of the location listener
        /// </summary>
        private ILocationServiceListener _loc = Resolver.Instance.Get<ILocationServiceListener>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBase"/> class.
        /// </summary>
        /// <param name="apiRelativePath">The relative path for API end-point</param>
        protected ApiBase(string apiRelativePath) : this(apiRelativePath, Settings.Instance.DsrLanguage, Settings.Instance.DsrCountryCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBase"/> class.
        /// </summary>
        /// <param name="apiRelativePath">The relative path for API end-point</param>
        /// <param name="lang">The language for the API</param>
        /// <param name="countryCode">The country for the API</param>
        protected ApiBase(string apiRelativePath, LanguagesEnum lang, CountryCodes countryCode)
        {
            this.Logger = Resolver.Instance.Get<ILog>();

            if (this.Logger == null)
            {
                this.Logger = new LogManager.DefaultLogger();
            }
            else
            {
                this.Logger.Initialize(this.GetType().FullName);
            }

            this.AddHeader(AuthorizationHeader, "Basic " + Resolver.Instance.Get<ISalesAppSession>().UserHash);

            this.ApiRelativePath = apiRelativePath;
            this.InitializeBaseUrl(countryCode);
            this.Lang = lang;
            this.CountryCode = countryCode;
            this._defaultHttpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseProxy = true,
                Proxy = WebRequest.DefaultWebProxy
            };
        }

        /// <summary>
        /// Gets or sets the base URL
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return this._baseUrl;
            }

            set
            {
                this._baseUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative path to be added to the base path
        /// </summary>
        public string ApiRelativePath { get; set; }

        /// <summary>
        /// Gets or sets API timeout in milliseconds
        /// </summary>
        protected ApiTimeoutEnum Timeout { get; set; }

        /// <summary>
        /// Gets the language for API calls
        /// </summary>
        protected LanguagesEnum Lang { get; private set; }

        /// <summary>
        /// Gets the country for API calls
        /// </summary>
        protected CountryCodes CountryCode { get; private set; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILog Logger { get; set; }

        /// <summary>
        /// Gets thee default HTTP client
        /// </summary>
        private HttpClient DefaultClient
        {
            get
            {
                return new HttpClient(this._defaultHttpHandler)
                {
                    Timeout = TimeSpan.FromMilliseconds((int)this.Timeout)
                };
            }
        }

        /// <summary>
        /// Gets the API path
        /// </summary>
        private string ApiPath
        {
            get { return this.BaseUrl + this.ApiRelativePath; }
        }

        /// <summary>
        /// Gets the get calls base path
        /// </summary>
        private string GetCallsBasePath
        {
            get { return this.ApiPath + "{0}"; }
        }

        /// <summary>
        /// Async method that allows you to send an object to the server and receive an object of the same type in response.
        /// </summary>
        /// <typeparam name="T">The type of object being passed in</typeparam>
        /// <param name="obj">The object to be serialized and sent to the server</param>
        /// <param name="successCallback">Method to be called on success</param>
        /// <param name="filterFlags">The error filter flags</param>
        /// <param name="timeOut">Timeout in seconds</param>
        /// <returns>An object of <typeparamref name="T"/> on successful communication with the server or null on failure</returns>
        [Obsolete("Maintained for backward compatability only. Do not use")]
        public virtual async Task<T> PostObjectAsync<T>(object obj, Action<object> successCallback = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal) where T : ServerResponseObjectsBase
        {
            ServerResponseObjectsBase response = await this.PostStringAsync(JsonConvert.SerializeObject(this.GetPostableData(obj), Formatting.Indented), successCallback, filterFlags, timeOut);

            if (response == null)
            {
                return default(T);
            }

            return response.GetResponseObject<T>();
        }

        /// <summary>
        /// Async method that allows you to send an object TP to the server and receive an object TR in response.
        /// </summary>
        /// <typeparam name="TR">The type of object returned by the API</typeparam>
        /// <typeparam name="TP">The type of object being passed to the API</typeparam>
        /// <param name="obj">The object to be serialized and sent to the server</param>
        /// <param name="successCallback">Method to be called on success</param>
        /// <param name="filterFlags">The error filter flags</param>
        /// <param name="timeOut">Timeout in seconds</param>
        /// <returns>An object of <typeparamref name="TR"/> on successful communication with the server or default(TR) on failure</returns>
        public virtual async Task<ServerResponse<TR>> PostObjectAsync<TR, TP>(TP obj, Action<object> successCallback = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
            where TR : class
        {
            this.Timeout = timeOut;
            this.Logger.Debug(string.Format("Passed object is null = {0}", obj == null));

            ServerResponse<TR> response = await this.PostJsonAsync<TR>(JsonConvert.SerializeObject(obj, Formatting.Indented), successCallback, filterFlags, timeOut);
            this.Logger.Debug(string.Format("Response is null = {0}", response == null));
            return response;
        }

        /// <summary>
        /// Makes get calls to the server.
        /// </summary>
        /// <typeparam name="TR">The expected return type</typeparam>
        /// <param name="value">Additional query parameters</param>
        /// <param name="successCallback">A method to be called on success</param>
        /// <param name="filterFlags">The API error filter flags</param>
        /// <param name="timeOut">Timeout in seconds</param>
        /// <returns>Server response that provides information on success/failure of the call and also encapsulates the returned object</returns>
        /// <exception cref="NotConnectedToInternetException">Throws an NotConnectedToInternetException if not connected to the internet when an API call is made</exception>
        /// <exception cref="Exception">Throw a System.Exception for any other error that occurs during processing</exception>
        public virtual async Task<ServerResponse<TR>> MakeGetCallAsync<TR>(string value, Action<object> successCallback = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            this.Timeout = timeOut;

            string url = string.Format(this.GetCallsBasePath, value);
            Logger.Verbose("Url " + url);

            var client = this.DefaultClient;
            foreach (KeyValuePair<string, string> headers in this._extraHeaders)
            {
                client.DefaultRequestHeaders.Add(headers.Key, headers.Value);
            }

            HttpResponseMessage responseMsg = null;
            try
            {
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    throw new NotConnectedToInternetException();
                }

                responseMsg = await client.GetAsync(await this.AddLocationAndLanguageToApi(url));

                this.NotifyIfErrorStatusCode((uint)responseMsg.StatusCode);
                var result = await this.GetServerResponsePackaged<TR>(responseMsg);

                if (filterFlags == ErrorFilterFlags.Ignore204)
                {
                    filterFlags = ErrorFilterFlags.AllowEmptyResponses;
                }

                if (!filterFlags.HasFlag(ErrorFilterFlags.AllowEmptyResponses))
                {
                    this.NotifyIfEmptyResponse(result.RawResponse, result.GetObject());
                }

                if (successCallback != null)
                {
                    successCallback(result);
                }

                return result;
            }
            catch (Exception ex)
            {
                ApiErrorHandler.ExceptionOccured(
                        this,
                        ex,
                        responseMsg != null ? (uint)responseMsg.StatusCode : 0,
                        filterFlags,
                        async x => await this.MakeGetCallAsync<TR>(value, successCallback, filterFlags),
                        successCallback);

                return new ServerResponse<TR>()
                {
                    IsSuccessStatus = false,
                    RequestException = ex,
                    Status = ServiceReturnStatus.ServerError
                };
            }
        }

        /// <summary>
        /// Method to send specific JSON String to the server and wait for a response
        /// </summary>
        /// <typeparam name="TR">Response type</typeparam>
        /// <param name="jsonString">JSON payload</param>
        /// <param name="successCallback">Success callback</param>
        /// <param name="filterFlags">Error handling flags</param>
        /// <param name="timeOut">Timeout in seconds</param>
        /// <returns>The server response</returns>
        public async Task<ServerResponse<TR>> PostJsonAsync<TR>(string jsonString, Action<object> successCallback = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            this.Timeout = timeOut;
            HttpResponseMessage responseMsg = null;
            this.Logger.Debug("Passed Json is as below");
            this.Logger.Debug(jsonString);
            try
            {
                // add all extra headers, if any
                HttpClient httpClient = this.DefaultClient;

                foreach (KeyValuePair<string, string> headers in this._extraHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(headers.Key, headers.Value);
                }

                string url = await this.AddLocationAndLanguageToApi(this.ApiPath);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, @"application/json");

                // check internet connection
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    throw new NotConnectedToInternetException();
                }

                responseMsg = await httpClient.PostAsync(url, content);

                this.NotifyIfErrorStatusCode((uint)responseMsg.StatusCode);

                var resp = new ServerResponse<TR>
                {
                    RawResponse = await responseMsg.Content.ReadAsStringAsync(),
                    StatusCode = responseMsg.StatusCode,
                    IsSuccessStatus = responseMsg.IsSuccessStatusCode
                };

                if (!filterFlags.HasFlag(ErrorFilterFlags.AllowEmptyResponses))
                {
                    this.NotifyIfEmptyResponse(resp.RawResponse, resp.GetObject());
                }

                if (successCallback != null)
                {
                    successCallback(resp);
                }

                return resp;
            }
            catch (Exception ex)
            {
                ApiErrorHandler.ExceptionOccured(
                    this,
                    ex,
                    responseMsg != null ? (uint)responseMsg.StatusCode : 0,
                    filterFlags,
                    async x => await PostJsonAsync<TR>(jsonString, successCallback),
                    successCallback);

                return new ServerResponse<TR>
                {
                    IsSuccessStatus = false,
                    RequestException = ex,
                    StatusCode =
                        responseMsg != null ? responseMsg.StatusCode : HttpStatusCode.ExpectationFailed,
                    RawResponse = "{}"
                };
            }
            finally
            {
                this.Logger.Debug("Exiting PostJsonAsync");
            }
        }

        /// <summary>
        /// Adds a header to the API request
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The header value</param>
        public void AddHeader(string name, string value)
        {
            if (this._extraHeaders.ContainsKey(name))
            {
                this._extraHeaders[name] = value;
            }
            else
            {
                this._extraHeaders.Add(name, value);
            }
        }

        /// <summary>
        /// Removes a header from the API request
        /// </summary>
        /// <param name="name">The name of the header to remove</param>
        public void RemoveHeader(string name)
        {
            if (!this._extraHeaders.ContainsKey(name))
            {
                return;
            }

            this._extraHeaders.Remove(name);
        }

        /// <summary>
        /// Method to be run after a up sync
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="postResponse">THe post response</param>
        /// <returns>A task of type Post response</returns>
        public async virtual Task<PostResponse> AfterSyncUpProcessing(object model, PostResponse postResponse)
        {
            return await Task.Run(() => postResponse);
        }

        /*/// <summary>
        /// Async method that allows you to send arbitrary strings to server
        /// </summary>
        /// <param name="value">The arbitrary string to be sent to the server</param>
        /// <param name="connectedToInternet">Flag for whether an internet connection is present. A <exception cref="NotConnectedToInternetException">NotConnectedToInternetException exception is throw if value is false</exception></param>
        /// <returns>ServerResponseObjectsBase on success or null on failure</returns>
        [Obsolete("ServerResponseObjectsBase is an unneeded class and will be removed, as will this method that has a dependancy on it. Use MakeGetCallAsync instead")]
        protected async Task<ServerResponseObjectsBase> GetStringAsync(string value, bool connectedToInternet)
        {
            string url = string.Format(GetCallsBasePath, value);
            var client = DefaultClient;
            foreach (KeyValuePair<string, string> headers in extraHeaders)
            {
                client.DefaultRequestHeaders.Add(headers.Key, headers.Value);
            }

            HttpResponseMessage responseMsg = null;

            try
            {
                if (!connectedToInternet)
                {
                    throw new NotConnectedToInternetException();
                }

                responseMsg = await client.GetAsync(await this.AddLocationAndLanguageToApi(url));

                string result = responseMsg.Content.ReadAsStringAsync().Result;
                if (responseMsg.IsSuccessStatusCode == false)
                {
                    throw new Exception(result);
                }

                return new ServerResponseObjectsBase
                {
                    RawResponseText = result,
                    StatusCode = responseMsg.StatusCode,
                    IsSuccessStatus = responseMsg.IsSuccessStatusCode
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Call error was " + ex.Message);
                return new ServerResponseObjectsBase
                {
                    IsSuccessStatus = false,
                    RequestException = ex,
                    StatusCode = responseMsg != null ? responseMsg.StatusCode : HttpStatusCode.ExpectationFailed,
                    RawResponseText = "{}"
                };
            }
        }*/

        /// <summary>
        /// Async method that allows you to send arbitrary JSON string to server
        /// </summary>
        /// <param name="jsonString">The arbitrary JSON string to be sent to the server</param>
        /// <param name="successCallback">The success callback</param>
        /// <param name="filterFlags">Error handling filter flags</param>
        /// <param name="timeOut">Time out in milliseconds</param>
        /// <returns>JSON string on success or null on failure</returns>
        protected async Task<ServerResponseObjectsBase> PostStringAsync(string jsonString, Action<object> successCallback = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            this.Timeout = timeOut;
            HttpResponseMessage responseMsg = null;
            var client = this.DefaultClient;
            foreach (KeyValuePair<string, string> headers in this._extraHeaders)
            {
                client.DefaultRequestHeaders.Add(headers.Key, headers.Value);
            }

            try
            {
                string url = await this.AddLocationAndLanguageToApi(this.ApiPath);
                responseMsg = await client.PostAsync(url, new StringContent(jsonString, Encoding.UTF8, @"application/json"));

                this.NotifyIfErrorStatusCode((uint)responseMsg.StatusCode);
                string rawResponse = await responseMsg.Content.ReadAsStringAsync();
                rawResponse.WriteLine();
                var resp = new ServerResponseObjectsBase
                {
                    RawResponseText = await responseMsg.Content.ReadAsStringAsync(),
                    StatusCode = responseMsg.StatusCode,
                    IsSuccessStatus = responseMsg.IsSuccessStatusCode
                };

                if (!filterFlags.HasFlag(ErrorFilterFlags.AllowEmptyResponses))
                {
                    this.NotifyIfEmptyResponse(resp.RawResponseText, null);
                }

                if (successCallback != null)
                {
                    successCallback(resp);
                }

                return resp;
            }
            catch (Exception ex)
            {
                ApiErrorHandler.ExceptionOccured(
                        this,
                        ex,
                        responseMsg != null ? (uint)responseMsg.StatusCode : 0,
                        filterFlags,
                        async x => await this.PostStringAsync(jsonString, successCallback),
                        successCallback);

                return new ServerResponseObjectsBase
                {
                    IsSuccessStatus = false,
                    RequestException = ex,
                    StatusCode = responseMsg != null ? responseMsg.StatusCode : HttpStatusCode.ExpectationFailed,
                    RawResponseText = "{}"
                };
            }
        }

        /// <summary>
        /// Get the base url
        /// </summary>
        /// <param name="country">The country for whose API to use</param>
        private void InitializeBaseUrl(CountryCodes country)
        {
            this.Logger.Debug(string.Format("Initializing API URL for country: {0}", country));
            this._baseUrl = this._configService.ApiUrl;

            this.Lang = this.Lang;
            this.CountryCode = country;
            this._defaultHttpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseProxy = true,
                Proxy = WebRequest.DefaultWebProxy
            };

            this.Logger.Debug(this._baseUrl);
        }

        /// <summary>
        /// Returns an object as API consumable data
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>The consumable data</returns>
        private Dictionary<string, string> GetPostableData(object obj)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Type type = obj.GetType();
            while (type != null)
            {
                foreach (var propInfo in type.GetTypeInfo().DeclaredProperties)
                {
                    if (propInfo.GetCustomAttribute<NotPostedAttribute>() == null)
                    {
                        result.Add(propInfo.Name, propInfo.GetValue(obj).ToString());
                    }
                }

                type = type.GetTypeInfo().BaseType;
            }

            return result;
        }

        /// <summary>
        /// Called to send a notification indicating an unexpected status code
        /// </summary>
        /// <param name="httpStatusCode">The status code</param>
        private void NotifyIfErrorStatusCode(uint httpStatusCode)
        {
            Type exceptionType = new[]
            {
                new { LBound = 401, UBound = 401, ExceptionType = typeof(UnauthorizedHttpException) },
                new { LBound = 400, UBound = 499, ExceptionType = typeof(HttpResponse400Exception) },
                new { LBound = 500, UBound = 599, ExceptionType = typeof(HttpResponse500Exception) },
                new { LBound = 204, UBound = 204, ExceptionType = typeof(HttpResponse204Exception) }
            }.Where(item => item.LBound <= httpStatusCode && item.UBound >= httpStatusCode)
                .Select(item => item.ExceptionType).FirstOrDefault();

            if (exceptionType == default(Type))
            {
                return;
            }

            Exception exception = Activator.CreateInstance(exceptionType) as Exception;
            if (exception == null)
            {
                return;
            }

            throw exception;
        }

        /// <summary>
        /// Called to send a notification indicating an empty response
        /// </summary>
        /// <param name="rawResponse">The server response</param>
        /// <param name="data">The data</param>
        private void NotifyIfEmptyResponse(string rawResponse, object data)
        {
            if (rawResponse.IsBlank())
            {
                throw new HttpResponse204Exception();
            }

            var enumerable = data as IEnumerable;
            if (enumerable != null)
            {
                if (!enumerable.OfType<object>().Any())
                {
                    throw new HttpResponse204Exception();
                }
            }
        }

        /// <summary>
        /// Return an packaged server response
        /// </summary>
        /// <typeparam name="TR">The given server response</typeparam>
        /// <param name="responseMessage">The server response message</param>
        /// <returns>The packages server response</returns>
        private async Task<ServerResponse<TR>> GetServerResponsePackaged<TR>(HttpResponseMessage responseMessage)
        {
            ServerResponse<TR> response = new ServerResponse<TR>
            {
                RawResponse = await responseMessage.Content.ReadAsStringAsync(),
                StatusCode = responseMessage.StatusCode,
                IsSuccessStatus = responseMessage.IsSuccessStatusCode
            };

            return response;
        }

        /// <summary>
        /// Add location information to the API call
        /// </summary>
        /// <param name="path">The API path</param>
        /// <returns>The API path with location added to it</returns>
        private async Task<string> AddLocationAndLanguageToApi(string path)
        {
            string currentLocation = await this._loc.GetLocation();
            string lang = Settings.Instance.DsrLanguage.ToString();

            if (path.ToLowerInvariant().Contains("?"))
            {
                path += "&lang=" + lang;
            }
            else
            {
                path += "?lang=" + lang;
            }

            if (!string.IsNullOrEmpty(currentLocation))
            {
                string cleanedpath = path + "?loc=" + currentLocation;
                if (path.ToLowerInvariant().Contains("?"))
                {
                    cleanedpath = path + "&loc=" + currentLocation;
                }

                return cleanedpath;
            }

            return path;
        }
    }
}