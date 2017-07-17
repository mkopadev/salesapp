using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Components;

namespace SalesApp.Droid.UI
{
    public class PeriodTypeItemListAdapter : ScrollItemListAdapter<string>
    {
        private bool HasResults;
          public PeriodTypeItemListAdapter(Activity context, List<string> items, bool hasResults)
            : base(context, items)
        {
            HasResults = hasResults;
        }

          public PeriodTypeItemListAdapter(Activity context, List<string> items)
            : base(context, items)
        {
            HasResults = true;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string item = GetItem(position).ToNetObject<string>();

            View view = convertView;

            view = Activity.LayoutInflater.Inflate(ItemLayoutResId, parent, false);

            TextView prospectname = view.FindViewById<TextView>(Resource.Id.prospect_row_title);

            if (HasResults)
            {
            }

            return view;
        }
    }
}
