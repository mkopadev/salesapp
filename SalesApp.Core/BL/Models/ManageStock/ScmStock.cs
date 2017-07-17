using System;
using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.ManageStock
{
    public class ScmStock
    {
        public string Name { get; set; }

        public Guid ProductTypeId { get; set; }

        public List<string> SerialNumbers { get; set; }
    }
}