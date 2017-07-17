using System;
using Newtonsoft.Json;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Json.Converters;

namespace SalesApp.Core.BL.Models.Chama
{
    public class Chamas : BusinessEntityBase
    {
         [JsonConverter(typeof(SerializedPropertyConveter))]
         public string Package { get; set; }

         public DateTime ServerTimeStamp { get; set; }
    }
}