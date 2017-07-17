using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SalesApp.Core.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector, bool ascending) where TSource : IComparable
        {
            List<TSource> sorted = source.OrderBy(keySelector).ToList();
            if (!ascending)
            {
                sorted.Reverse();
            }

            for (int i = 0; i < sorted.Count(); i++)
            {
                source.Move(source.IndexOf(sorted[i]), i);
            }
        }
    }
}
