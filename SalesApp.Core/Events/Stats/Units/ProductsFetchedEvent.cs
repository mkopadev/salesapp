using System;
using System.Collections.Generic;
using SalesApp.Core.BL;

namespace SalesApp.Core.Events.Stats.Units
{
    public class ProductsFetchedEvent : EventArgs
    {
        public List<Product> Products { get; private set; }

        public ProductsFetchedEvent(List<Product> products)
        {
            this.Products = products;
        }
    }
}