using System;

namespace SalesApp.Core.Api
{
    public class ProductDto
    {
        public Guid ProductTypeId { get; set; }
        public string SerialNumber { get; set; }
        public string DisplayName { get; set; }
        public string DateAcquired { get; set; }

    }
}