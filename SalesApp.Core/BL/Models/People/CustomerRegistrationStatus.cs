using System;
using SalesApp.Core.BL.Contracts;
using SQLiteNetExtensions.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    public class CustomerRegistrationStepsStatus : BusinessEntityBase
    {
        public int RequestStatus { get; set; }
        
        [ForeignKey(typeof(Customer))]
        public Guid CustomerId { get; set; }

        [ForeignKey(typeof(Product))]
        public Guid ProductId { get; set; }

        public string StepName { get; set; }

        public int StepNumber { get; set; }

        public string StepStatus { get; set; }

        [ManyToOne]
        public Customer Customer { get; set; }

        [ManyToOne]
        public Product Product { get; set; }

        public Guid CustomerStatusId { get; set; }

        public string AdditionalInfo { get; set; }
    }
}