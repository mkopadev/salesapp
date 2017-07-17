namespace SalesApp.Core.BL.Models.TicketList
{
    /// <summary>
    /// A concrete class representing a customer ticket
    /// </summary>
    public class CustomerTicket : AbstractTicketBase
    {
        /// <summary>
        /// Gets or sets the name of the customer
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the customer phone number
        /// </summary>
        public string CustomerPhone { get; set; }

        /// <summary>
        /// Gets or sets the customer's latest product
        /// </summary>
        public string Product { get; set; }
    }
}