using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.Logging;
using SalesApp.Droid.Components;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.People.Prospects;
using Exception = System.Exception;

namespace SalesApp.Droid.People
{
    class PersonListFilter<TListItemType> : Filter where TListItemType : IPersonItem
    {
        private static readonly ILog Logger = LogManager.Get(typeof(PersonListFilter<TListItemType>));

        private PersonSectionListAdapter<TListItemType> sectionedListAdapter;
        private readonly Activity _context;



        public PersonListFilter(PersonSectionListAdapter<TListItemType> sectionedListAdapter)
        {
            this.sectionedListAdapter = sectionedListAdapter;
            _context = sectionedListAdapter.Activity;
        }



        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            try
            {
                if (constraint == null)
                {
                    return new FilterResults();
                }
                var returnObj = new FilterResults();

                if (constraint.Length() < 1)
                {
                    returnObj.Values = sectionedListAdapter.Original.ToJavaObject();
                    returnObj.Count = sectionedListAdapter.Original.Count;
                    return returnObj;
                }


                if (sectionedListAdapter.Original == null)
                    sectionedListAdapter.Original = sectionedListAdapter.Sections;

                if (sectionedListAdapter.Original != null && sectionedListAdapter.Original.Any())
                {

                    List<PersonSectionListAdapter<TListItemType>.ListSection> found =
                        new List<PersonSectionListAdapter<TListItemType>.ListSection>();
                    foreach (PersonSectionListAdapter<TListItemType>.ListSection s in sectionedListAdapter.Original)
                    {
                        List<TListItemType> foundObjects = new List<TListItemType>();
                        for (int i = 0; i < s.Adapter.Count; i++)
                        {
                            TListItemType f = s.Adapter.GetItem(i).ToNetObject<TListItemType>();
                            if (f.GetFilterString().ToLower().Contains(constraint.ToString().ToLower()))
                            {
                                foundObjects.Add(f);
                            }
                        }

                        if (foundObjects.Count > 0)
                        {

                            found.Add
                                (
                                    new PersonSectionListAdapter<TListItemType>.ListSection
                                        (
                                        s.Header
                                        , GetBaseAdapter(foundObjects)
                                        , s.HeaderColor
                                        )
                                );
                        }
                    }

                    if (found.Count > 0)
                    {
                        returnObj.Values = found.ToJavaObject();
                        returnObj.Count = found.Count;
                    }
                    else
                    {
                        returnObj.Values = sectionedListAdapter.Empty.ToJavaObject();
                        returnObj.Count = 1;
                    }

                }
                else
                {
                    return new FilterResults();
                }

                if (constraint != null) constraint.Dispose();
                return returnObj;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }



        }

        private BaseAdapter GetBaseAdapter(List<TListItemType> foundObjects)
        {
            Type typeOfItem = typeof(TListItemType);
            if (typeOfItem == typeof(ProspectItem))
            {
                return new ProspectItemListAdapter
                    (
                    _context
                    ,
                    JsonConvert.DeserializeObject<List<ProspectItem>>
                        (
                            JsonConvert.SerializeObject(foundObjects)
                        )
                    );
            }
            else //if (typeOfItem == typeof(CustomerItem))
            {
                return new CustomerItemListAdapter
                (
                    _context
                    ,
                    JsonConvert.DeserializeObject<List<CustomerItem>>
                    (
                        JsonConvert.SerializeObject(foundObjects)
                    )
                );
            }
        }

        protected override void PublishResults(ICharSequence constraint, FilterResults filterResults)
        {
            if (filterResults != null && filterResults.Values != null && filterResults.Count > 0)
            {
                List<PersonSectionListAdapter<TListItemType>.ListSection> sectionList = filterResults.Values.ToNetObject<List<PersonSectionListAdapter<TListItemType>.ListSection>>();
                sectionedListAdapter.Sections = sectionList;
            }
            else
            {
                sectionedListAdapter.Sections = null;
            }

            // TODO sort out why this error occurs, has to do with no items in array
            try
            {
                sectionedListAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            constraint.Dispose();
            if (filterResults != null)
            {
                filterResults.Dispose();
            }
        }

    }
}
