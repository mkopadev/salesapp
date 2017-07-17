using System;
using System.Collections.Generic;
using SalesApp.Core.Enums;

namespace SalesApp.Core.Api.SwapComponent
{
    public class CustomerDetailsResponse
    {
        public Guid Id { get; set; }
        public string Surname { get; set; }
        public string OtherNames { get; set; }
        public string IdentificationNumber { get; set; }
        public string PhoneNumber { get; set; }
        public List<SwapProduct> Products { get; set; }
        public bool CustomerFound { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public int IdentifierType { get; set; }
        public string Identifier { get; set; }
        public bool Successful { get; set; }
        public ServiceReturnStatus Status { get; set; }
    }

    public enum AccountStatus
    {
        Active = 1,
        Blocked = 2,
        Cancelled = 3,
    }
}
