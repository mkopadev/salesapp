using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Payments
{
    public class PaymentsServerResponse
    {
        /// <summary>
        /// Gets or sets a small summary info
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the total payment
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Gets or sets the list of payments
        /// </summary>
        public List<Payment> Payments { get; set; }
    }
}