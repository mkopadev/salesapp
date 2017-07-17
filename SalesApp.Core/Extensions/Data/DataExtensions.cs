using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SalesApp.Core.Extensions.Data
{
    public static class DataExtensions
    {
        public static TResult CastTo<TResult>(this ICastable source)
        {
            return JsonConvert.DeserializeObject<TResult>(JsonConvert.SerializeObject(source));
        }

        public static List<TResult> NotIntersecting<TResult>(this List<TResult> firstList, List<TResult> secondList)
        {
            return firstList.Except(secondList).Union(secondList.Except(firstList)).ToList();
        }

        
    }
}