using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.People
{
    public abstract class PersonSectionListAdapter<TItemListType> : BaseAdapter<PersonSectionListAdapter<TItemListType>.ListSection>, IFilterable where TItemListType : IPersonItem
    {
        private Filter _filter;
        private const int TypeSectionHeader = 0;

        public LayoutInflater Inflater { get; set; }

        public PersonSectionListAdapter(Activity activity)
        {
            Sections = new List<ListSection>();
            Original = new List<ListSection>();
            Empty = new List<ListSection>();
            Activity = activity;

            Inflater = LayoutInflater.From(Activity);

        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            foreach (ListSection s in Sections)
            {
                if (position == 0)
                {
                    if (!(view is LinearLayout))
                    {
                        view = Inflater.Inflate(Resource.Layout.layout_list_prospect_title, parent, false);
                    }
                    TextView title = view.FindViewById<TextView>(Resource.Id.list_title);
                    title.Text = s.Header;
                    title.SetBackgroundResource(s.HeaderColor);
                    return view;
                }
                int size = s.Adapter.Count + 1;
                if (position < size)
                {
                    return s.Adapter.GetView(position - 1, convertView, parent);
                }

                position -= size;
            }
            return null;
        }

        public override long GetItemId(int position) { return position; }

        public override int GetItemViewType(int position)
        {
            int typeOffset = TypeSectionHeader + 1;
            foreach (ListSection s in Sections)
            {
                if (position == 0) return TypeSectionHeader;
                int size = s.Adapter.Count + 1;
                if (position < size) return (typeOffset + s.Adapter.GetItemViewType(position - 1));
                position -= size;
                typeOffset += s.Adapter.ViewTypeCount;
            }
            return -1;
        }

        public override ListSection this[int index]
        {
            get { return Sections[index]; }
        }

        public List<ListSection> Sections { get; set; }

        public List<ListSection> Original { get; set; }

        public List<ListSection> Empty { get; set; }

        public Activity Activity { get; set; }

        public override bool IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }

        public override int Count
        {
            get
            {
                // count all items in all sections
                if (Sections == null)
                {
                    return 0;
                }
                return Sections.Sum(s => s.Adapter.Count + 1);
            }
        }

        public override int ViewTypeCount
        {
            get
            {
                // return the number of views to be created by all sections
                if (Sections == null)
                {
                    return 0;
                }
                return 1 + Sections.Sum(s => s.Adapter.ViewTypeCount);
            }
        }

        public void AddSection(string header, BaseAdapter adapter, int headcolor)
        {

            var listSection = new ListSection(header, adapter, headcolor);
            bool issection = false, isoriginal = false;

            foreach (var section in Sections)
            {
                if (section.Header.Equals(listSection.Header))
                    issection = true;
            }

            foreach (var original in Original)
            {
                if (original.Header.Equals(listSection.Header))
                    isoriginal = true;
            }


            if (!isoriginal)
                Original.Add(listSection);

            if (!issection)
                Sections.Add(listSection);
        }

        internal void ClearSections()
        {
            if (Sections != null)
            {
                Sections.Clear();
            }
            if (Original != null)
            {
                Original.Clear();
            }
            if (Empty != null)
            {
                Empty.Clear();
            }
        }

        public class ListSection
        {
            public ListSection(string header, BaseAdapter adapter, int headerColor = 0)
            {
                Adapter = adapter;
                HeaderColor = headerColor;
                Header = header;
            }

            public string Header { get; set; }
            public BaseAdapter Adapter { get; set; }
            public int HeaderColor { get; set; }

        }

        //TODO move this to more generic list adapter class
        public override Object GetItem(int position)
        {
            foreach (ListSection s in Sections)
            {
                if (position == 0)
                    return s.ToJavaObject();

                int size = s.Adapter.Count + 1;
                if (position < size)
                    return s.Adapter.GetItem(position - 1);

                position -= size;
            }
            return null;
        }



        public Filter Filter
        {
            get
            {
                if (_filter == null)
                {
                    _filter = new PersonListFilter<TItemListType>(this);
                }
                return _filter;
            }
        }


    }


}