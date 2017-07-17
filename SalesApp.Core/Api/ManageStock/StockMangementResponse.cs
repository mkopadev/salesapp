using System.Collections.Generic;

namespace SalesApp.Core.Api.ManageStock
{
    public class StockMangementResponse
    {
        public string Name { get; set; }

        public string ProductTypeId { get; set; }

        public List<string> Units { get; set; }
    }
}