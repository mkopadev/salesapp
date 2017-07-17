using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Commissions.Retainer
{
    public class RetainerCommissionResponse
    {
        public string Info { get; set; }

        /*
        public double Sales { get; set; }*/

        public string Total { get; set; }

        public List<RetainerCommissionItem> Retainer { get; set; }
    }
}