using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.UI
{
    public class HintAdapter : ArrayAdapter<string>
    {
        private bool showHint = false;

        public HintAdapter(Context ctx, List<string> items)
            : base(ctx, Resource.Layout.spinner_default, items)
        {
            SetDropDownViewResource(Resource.Layout.spinner_default_dropdown);
        }

        public override int Count
        {
            get
            {
                return base.Count > 0 && showHint ? base.Count - 1 : base.Count;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            bool isSpinner = parent is Spinner;

            if (!showHint && isSpinner)
            {
                Spinner spinner = parent as Spinner;
                if (!string.IsNullOrWhiteSpace(spinner.Prompt))
                {
                    Insert(spinner.Prompt, 0);
                    showHint = true;
                    spinner.SetSelection(base.Count - 1);
                }
            }

            if (showHint && position >= base.Count - 1)
            {
                View hintView = base.GetView(0, convertView, parent);
                if (hintView is TextView)
                {
                    // (hintView as TextView).SetTextAppearance(Context, hintResourceId);
                }

                return hintView;
            }

            if (showHint && position < base.Count - 1)
            {
                return base.GetView(position + 1, convertView, parent);
            }

            return base.GetView(position, convertView, parent);
        }

        public override long GetItemId(int position)
        {
            if (showHint)
            {
                // if at the last item
                /* if (position >= base.Count - 1)
                {
                    return -1;
                } */

                return base.GetItemId(position + 1);
            }

            return base.GetItemId(position);
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            if (showHint)
            {
                // make the hint visible
                return base.GetDropDownView(position + 1, convertView, parent);
            }

            return base.GetDropDownView(position, convertView, parent);
        }
    }
}