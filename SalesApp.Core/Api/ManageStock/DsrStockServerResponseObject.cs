using System;
using System.Collections.Generic;
using SalesApp.Core.BL.Models.ManageStock;

namespace SalesApp.Core.Api.ManageStock
{
    public class DsrStockServerResponseObject
    {
        public int UnitsAllocated { get; set; }

        public int MaxAllowedUnits { get; set; }

        public Guid PersonId { get; set; }

        public Guid PersonRoleId { get; set; }

        public RequestStatus Status { get; set; }

        public List<KeyValue> DsrDetails { get; set; }

        public List<DsrProductAllocationItem> Products { get; set; }
    }
}