using System.Collections.Generic;
using Android.App;
using Android.Widget;
using Java.Lang;

namespace SalesApp.Droid.Components
{
    public abstract class ListAdapter<T> : BaseAdapter<T>
    {
        private Activity _activity;
        private bool _hasItems;
        private List<T> _items;

        protected Activity Activity { get { return _activity; } }

        protected List<T> Items { get { return _items; } }

        public ListAdapter(Activity activity, List<T> items)
        {
            _activity = activity;
            _hasItems = items != null && items.Count > 0;
            _items = items;
            
        }
        public override Object GetItem(int position)
        {
            if (position < _items.Count)
                return _items[position].ToJavaObject();
            return null;
        }

        public override T this[int position]
        {
            get
            {
                if (_items != null)
                    return _items[position];

                return default(T);
            }
        }

        public override int Count
        {
            get {
                if (_items != null)
                    return _items.Count;

                return 0;
            }
        }
    }
}