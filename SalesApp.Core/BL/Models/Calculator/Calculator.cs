using System;
using System.Collections.Generic;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Calculator
{
    /*
    saves calc products their meta data in db to be used when offline
    */
    public class Calculator : BusinessEntityBase
    {
         public string Products { get; set; }

        public static implicit operator Calculator(List<Calculator> v)
        {
            throw new NotImplementedException();
        }
    }
}