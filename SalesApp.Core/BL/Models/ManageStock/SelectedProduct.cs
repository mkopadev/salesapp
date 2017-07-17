using System;
using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.ManageStock
{
    public class SelectedProduct
    {
        public string DsrPhone { get; set; }

        public Guid PersonId { get; set; }

        public Guid PersonRoleId { get; set; }

        public List<ScmStock> Units { get; set; }
    }
}