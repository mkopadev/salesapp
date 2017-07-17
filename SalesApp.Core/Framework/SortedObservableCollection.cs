using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SalesApp.Core.BL.Models;

namespace SalesApp.Core.Framework
{
    public class SortedObservableCollection<T> : ObservableCollection<T> where T : IListSectionItem
    {
        public SortedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public new void Add(T item)
        {
            List<T> list = this.ToList();

            int index = list.FindLastIndex(x => x.SectionHeader == item.SectionHeader);

            if (index < 0)
            {
                /*
                    item is the first of its kind, show
                    Todo We may also want to sort sections, for now we dont
                */

                item.IsSectionHeader = true;
            }

            // Put the new item as the last in its section
            this.Insert(index + 1, item);
        }
    }
}