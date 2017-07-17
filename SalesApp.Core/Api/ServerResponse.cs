using System;
using System.Net;
using Newtonsoft.Json;
using SalesApp.Core.Enums;

namespace SalesApp.Core.Api
{
    public class ServerResponse<TR>
    {
        /// <summary>
        /// HTTP Status Code for the API call.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Raw text response of the API call.
        /// </summary>
        public string RawResponse { get; set; }

        /// <summary>
        /// Boolean indicating of the request was succesful.
        /// </summary>
        public bool IsSuccessStatus { get; set; }

        /// <summary>
        /// Exception (if any) raised during the API call.
        /// </summary>
        public Exception RequestException { get; set; }

        /// <summary>
        /// The return code from the API service.
        /// </summary>
        public ServiceReturnStatus Status { get; set; }

        public TR GetObject()
        {
            if (this.IsSuccessStatus)
            {
                return JsonConvert.DeserializeObject<TR>(this.RawResponse);
            }

            return default(TR);
        }
    }
}