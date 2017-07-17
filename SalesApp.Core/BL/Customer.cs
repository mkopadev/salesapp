using System;
using System.Collections.Generic;

namespace Mkopa.Core.BL
{
    public class Customer
    {
        // TODO use Sync Base Class when merged with other feature branch
        public string DsrPhone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string NationalId { get; set; }
        public Product Product { get; set; }
        public Guid RequestId { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}