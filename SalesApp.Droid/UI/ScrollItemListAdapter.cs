using System.Collections.Generic;
using Android.App;
using Android.Widget;
using Java.Lang;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.UI
{
    public abstract class ScrollItemListAdapter<TListItemType> : BaseAdapter<TListItemType>
    {

        public bool HasResults { get; set; }

        public Activity Activity { get; set; }
        public int ItemLayoutResId { get { return Resource.Layout.layout_list_prospect_row; } }

        private List<TListItemType> _items;
        protected Settings settings;

        public ScrollItemListAdapter(Activity activity, List<TListItemType> items)
        {
            Activity = activity;
            this.HasResults = items != null && items.Count > 0;
            _items = items;

            settings = Settings.Instance;
        }

        public override TListItemType this[int index] { get { return _items[index]; } }

        public override int Count
        {
            get
            {
                if (_items == null)
                {
                    return 0;
                }
                return _items.Count;
            }
        }

        public override long GetItemId(int position) { return position; }




        //TODO move this to generic list adapter class
        public override Object GetItem(int position)
        {
            if (position < _items.Count) return _items[position].ToJavaObject();
            return null;
        }
    }
}