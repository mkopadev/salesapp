using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Adjustments
{
    public class AdjustmentServerResponse
    {
        /// <summary>
        /// Gets or sets the adjustments' additional information
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or set the total amount of adjustments
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Get or sets the list of adjustments
        /// </summary>
        public List<Adjustment> Adjustments { get; set; }
    }
}