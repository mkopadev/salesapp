using Android.App;
using Android.OS;
using Android.Views;

namespace SalesApp.Droid.UI.Utils
{
    public class DialogFragment1 : DialogFragment
    {
        public static DialogFragment1 NewInstance(Bundle bundle)
        {
            DialogFragment1 fragment = new DialogFragment1();
            fragment.Arguments = bundle;
            return fragment;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.fragment_alert_dialog_fragement_builder, container, false);
            /*Button button = view.FindViewById<Button>(Resource.Id.CloseButton);
            button.Click += delegate {
                Dismiss();
                Toast.MakeText(Activity, "Dialog fragment dismissed!", ToastLength.Short).Show();
            };*/
            return view;
        }
    }
}