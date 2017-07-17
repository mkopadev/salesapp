namespace SalesApp.Core.BL.Models.Commissions.Payments
{
    public class Payment
    {
        /// <summary>
        /// Gets or sets the date the payment was done
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the M-Pesa refence number for the payment
        /// </summary>
        public string RefNo { get; set; }

        /// <summary>
        /// Gets or sets the amount of the payment
        /// </summary>
        public double Amount { get; set; }
    }
}