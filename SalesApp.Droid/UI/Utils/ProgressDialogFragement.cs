using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.UI.Utils
{
    public class ProgressDialogFragement : DialogFragment, View.IOnClickListener
    {
        private int _buttons { get; set; }

        private string _message { get; set; }

        private string _title { get; set; }

        private TextView txtMessage;

        private static string msg = "MESSAGE";
        private static string tit = "TITLE";

        public ProgressDialogFragement()
        {

        }

        public static ProgressDialogFragement NewInstance(string message, string title)
        {
            var fragment = new ProgressDialogFragement { Arguments = new Bundle() };
            fragment.Arguments.PutString(msg, message);
            fragment.Arguments.PutString(tit, title);
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (this.Arguments != null)
            {
                _message = this.Arguments.GetString(msg);
                _title = this.Arguments.GetString(tit);
                if (string.IsNullOrEmpty(_title))
                {
                    this.SetStyle(StyleNoTitle, 0);
                }
            }
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Progress Dialog");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.progress_dialog_layout, container, false);
            txtMessage = (TextView)rootView.FindViewById(Resource.Id.txtAlertMessage);
            txtMessage.Text = _message;
            return rootView;
        }

        public void OnClick(View v)
        {
           
        }
    }
}