using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.People
{
    public abstract class PersonItemListAdapter<TListItemType> : BaseAdapter<TListItemType>
    {
        public bool HasResults { get; set; }

        public Activity Activity { get; set; }
        public int ItemLayoutResId { get { return Resource.Layout.layout_list_prospect_row; } }

        private List<TListItemType> _items;

        protected ViewHolder viewHolder;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            if (view == null || !(view is RelativeLayout))
            {
                viewHolder = new ViewHolder();
                view = Activity.LayoutInflater.Inflate(ItemLayoutResId, parent, false);
                viewHolder.PersonName = view.FindViewById<TextView>(Resource.Id.prospect_row_title);
                viewHolder.PersonPhoneNo = view.FindViewById<TextView>(Resource.Id.prospect_row_phoneno);
                viewHolder.ProspectReminderTime = view.FindViewById<TextView>(Resource.Id.prospect_row_time);
                viewHolder.ProspectStatus = view.FindViewById<ImageView>(Resource.Id.imgView4);
                viewHolder.SyncStatus = view.FindViewById<ImageView>(Resource.Id.imgStatus);
                view.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)view.Tag;
            }

            return view;
        }

        public PersonItemListAdapter(Activity activity, List<TListItemType> items)
        {
            Activity = activity;
            this.HasResults = items != null && items.Count > 0;
            _items = items;
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

        protected class ViewHolder : Object
        {
            public TextView PersonName;
            public TextView PersonPhoneNo;
            public TextView ProspectReminderTime;
            public ImageView ProspectStatus;
            public ImageView SyncStatus;
        }
    }
}