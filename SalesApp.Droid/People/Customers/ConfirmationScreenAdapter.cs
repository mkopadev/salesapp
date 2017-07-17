using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.ManageStock;

namespace SalesApp.Droid.People.Customers
{
    public class ConfirmationScreenAdapter : BaseAdapter<KeyValue>
    {
        private List<GroupKeyValue> _details;
        private Activity _context;
        private TextView _tvKey;
        private TextView _tvValue;

        public ConfirmationScreenAdapter(Activity context, List<GroupKeyValue> details)
        {
            this._details = details;
            this._context = context;
        }

        public override KeyValue this[int position]
        {
            get { return this._details.ElementAt(position); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is available
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.confirmation_screen_list_item, null);
                _tvKey = (TextView)view.FindViewById(Resource.Id.tvKey);
                _tvValue = (TextView)view.FindViewById(Resource.Id.tvValue);
            }
            _tvKey.Text = this._details[position].Key;
            _tvValue.Text = this._details[position].Name;
            return view;
        }

        public override int Count
        {
            get { return this._details.Count; }
        }
    }
}