using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.Commissions.Quality
{
    public class QualityCommissionItem
    {
        [JsonProperty("Month")]
        public string Date { get; set; }

        /*[JsonProperty("QualitySale")]
        public double Sales { get; set; }*/

        public double Commissions { get; set; }
    }
}