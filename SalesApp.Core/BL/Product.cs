using System;
using Newtonsoft.Json;
using SalesApp.Core.BL.Contracts;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL
{
    public class Product : BusinessEntityBase
    {
        public Guid ProductTypeId { get; set; }

        [JsonProperty("Name")]
        public string DisplayName { get; set; }

        [JsonProperty("DisplayName")]
        [Ignore]
        public string ProductName { get; set; } // HACK. To be amended later. Hold object for product name in down sync. Dont use it anyother place instead 'DisplayName'

        public string SerialNumber { get; set; }

        public string DateAcquired { get; set; }
    }
}