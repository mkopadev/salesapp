namespace SalesApp.Core.BL.Models.Commissions.Tax
{
    public class CommissionTaxServerResponse
    {
        /// <summary>
        /// Gets or sets the adjustments' additional information
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the taxation
        /// </summary>
        public Taxation Taxation { get; set; }
    }

    public class Taxation
    {
        /// <summary>
        /// Gets or sets the total earnings
        /// </summary>
        public double Earnings { get; set; }

        /// <summary>
        /// Gets or sets the total tax
        /// </summary>
        public double Tax { get; set; }
    }
}