using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.Components.UIComponents.CustomInfo
{
    public class CustomDialogFragment : DialogFragment
    {
        Button positive, negative;
        private string str_title, str_text;
        private string str_pos, str_neg, str_other;

        public CustomDialogFragment(String title, String text, String btn1, String btn2, String btn3)
        {
            this.str_title = title;
            this.str_text = text;
            this.str_pos = btn1;
            this.str_neg = btn2;
            this.str_other = btn3;
        }

        public interface CustomDialogListener
        {
            void onCustomDialogSelected(int selection);
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            View view = inflater.Inflate(Resource.Layout.layout_custom_dialog, container);

            TextView text = (TextView)view.FindViewById(Resource.Id.tvmessagedialogtext);
            TextView title = (TextView)view.FindViewById(Resource.Id.tvmessagedialogtitle);

            positive = (Button)view.FindViewById(Resource.Id.bmessageDialogYes);
            positive.Click += positiveBtnClick;
            if (str_pos == null)
                positive.Visibility = ViewStates.Gone;
            else
                positive.Text = str_pos;

            negative = (Button)view.FindViewById(Resource.Id.bmessageDialogNo);
            negative.Click += negativeBtnClick;
            if (str_neg == null)
                negative.Visibility = ViewStates.Gone;
            else
                negative.Text = str_neg;

            if (str_title == null)
                title.Visibility = ViewStates.Gone;
            else
                title.Text = str_title;

            text.Text = str_text;

            return view;

        }

        public override void OnResume()
        {
            // Auto size the dialog based on it's contents
            Dialog.Window.SetLayout(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);

            // Make sure there is no background behind our view
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            base.OnResume();
        }

        private void negativeBtnClick(object sender, EventArgs e)
        {
            ((CustomDialogListener)this.Activity).onCustomDialogSelected(-1);
        }

        private void positiveBtnClick(object sender, EventArgs e)
        {
            ((CustomDialogListener)this.Activity).onCustomDialogSelected(1);
        }



    }



}