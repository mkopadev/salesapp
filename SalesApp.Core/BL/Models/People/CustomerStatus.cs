using System.Collections.Generic;
using SalesApp.Core.BL.Contracts;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    public class CustomerStatus : BusinessEntityBase
    {
        public int RequestStatus { get; set; }

        public string CustomerName { get; set; }

        public string CustomerProduct { get; set; }

        public string CustomerPhone { get; set; }

        [Ignore]
        public List<CustomerRegistrationStep> Steps { get; set; } 

        [Ignore]
        public string CustomerNotFound { get; set; }

        public string AdditionalInfo { get; set; }

        public string AccountStatus { get; set; }
    }
}