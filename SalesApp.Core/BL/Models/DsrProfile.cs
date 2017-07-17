using System;
using Newtonsoft.Json;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models
{
    //[Preserve(AllMembers = true)]
    public class DsrProfile : BusinessEntityBase
    {
        public string DsrPhone { get; set; }

        public int OfflineLoginAttempts { get; set; }

        public DateTime LastOnlineLogin { get; set; }

        public DateTime LastOfflineLogin { get; set; }

        [JsonProperty("Hash")]
        public string PinHash { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
