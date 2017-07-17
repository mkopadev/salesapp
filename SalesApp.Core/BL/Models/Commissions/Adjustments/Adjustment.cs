using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.Commissions.Adjustments
{
    public class Adjustment
    {
        /// <summary>
        /// Gets or sets the date the adjustment was made
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the name of the adjustment
        /// </summary>
        [JsonProperty("Adjustment")]
        public string AdjustmentName { get; set; }

        /// <summary>
        /// Gets or sets the amount the adjustment
        /// </summary>
        public double Amount { get; set; }
    }
}