using System;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;

namespace SalesApp.Core.Api.Person
{
    public class CustomerRegistrationResponse : PostResponse
    {
        public Customer Customer { get; set; }

        [JsonProperty("Successful")]
        public bool RegistrationSuccessful { get; set; }

        public string CustomerId;

        public Guid RegistrationId { get; set; }

        public int Retry { get; set; }
    }
}