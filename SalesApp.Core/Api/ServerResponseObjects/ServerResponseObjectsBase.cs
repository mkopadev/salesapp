using System;
using System.Net;
using Newtonsoft.Json;
using SalesApp.Core.Api.Attributes;
using SQLite.Net.Attributes;

namespace SalesApp.Core.Api.ServerResponseObjects
{
    public class ServerResponseObjectsBase
    {
        [Ignore]
        [NotPosted]
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        public string RawResponseText { get; set; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        public bool IsSuccessStatus { get; set; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        public Exception RequestException { get; set; }

        public T GetResponseObject<T>()
        {
            T responseObject = JsonConvert.DeserializeObject<T>(RawResponseText);

            (responseObject as ServerResponseObjectsBase).IsSuccessStatus = IsSuccessStatus;
            (responseObject as ServerResponseObjectsBase).RawResponseText = RawResponseText;
            (responseObject as ServerResponseObjectsBase).StatusCode = StatusCode;

            return responseObject;
        }

    }
}