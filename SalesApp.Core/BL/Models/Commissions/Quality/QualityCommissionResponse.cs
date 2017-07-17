using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Quality
{
    public class QualityCommissionResponse
    {

        public string Info { get; set; }

        public double Total { get; set; }

        public List<QualityCommissionItem> QualityCommission { get; set; }
    }
}