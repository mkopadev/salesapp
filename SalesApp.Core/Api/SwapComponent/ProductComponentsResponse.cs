using System.Collections.Generic;

namespace SalesApp.Core.Api.SwapComponent
{
    public class ProductComponentsResponse
    {
        public List<ProductComponent> ProductComponents { get; set; }
        public bool Successful { get; set; }
        public string ResponseText { get; set; }
    }
}
