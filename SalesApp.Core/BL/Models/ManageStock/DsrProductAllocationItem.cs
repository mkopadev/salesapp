using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.ManageStock
{
    public class DsrProductAllocationItem
    {
        [JsonProperty("Name")]
        public string ProductName { get; set; }

        public Guid ProductTypeId { get; set; }

        public List<DeviceAllocationItem> Units { get; set; }
    }
}