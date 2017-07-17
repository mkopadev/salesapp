using System;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// Class represents a product assigned to a prospect
    /// </summary>
    public class ProspectProduct : Product
    {
        /// <summary>
        /// Gets or sets the Id for the prospect who owns this product
        /// </summary>
        public Guid ProspectId { get; set; }
    }
}