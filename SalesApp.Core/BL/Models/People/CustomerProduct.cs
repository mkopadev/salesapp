using System;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// Class represents a product assigned to a customer
    /// </summary>
    public class CustomerProduct : Product
    {
        /// <summary>
        /// Gets or sets the Id for the customer who owns this product
        /// </summary>
        public Guid CustomerId { get; set; }
    }
}