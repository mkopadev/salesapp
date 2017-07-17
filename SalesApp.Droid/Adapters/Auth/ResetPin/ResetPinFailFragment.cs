using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.Adapters.Auth.ResetPin
{
    public class ResetPinFailFragment : Fragment
    {
        Button btnTryAgain, btnCancelReset;
        private Activity activity;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public interface ResetPinFailFragmentListener
        {
            void onFailSelected(int selection);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this.activity = activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_resetpin_fail, container, false);

            btnTryAgain = (Button)view.FindViewById(Resource.Id.btnTryAgain);
            btnTryAgain.Click += btnTryAgain_Click;

            btnCancelReset = (Button)view.FindViewById(Resource.Id.btnCancelReset);
            btnCancelReset.Click += btnCancelReset_Click;
            return view;
        }

        private void btnTryAgain_Click(object sender, EventArgs e)
        {
            ((ResetPinFailFragmentListener)activity).onFailSelected(-1);
        }

        private void btnCancelReset_Click(object sender, EventArgs e)
        {
            ((ResetPinFailFragmentListener)activity).onFailSelected(1);
        }
    }
}
