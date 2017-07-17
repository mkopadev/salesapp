using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.Adapters.Auth.ResetPin
{
    public class ResetPinSuccessfulFragment : Fragment
    {

        Button button;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public interface ResetPinSuccesfulFragmentListener
        {
            void onSuccessSelected(int p);
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_resetpin_success, container, false);
            button = (Button)view.FindViewById(Resource.Id.btnBackToLogin);
            button.Click += btnBackToLogin_Click;
            return view;
        }

        private void btnBackToLogin_Click(object sender, EventArgs e)
        {
            //activity.OnBackPressed();
            ((ResetPinSuccesfulFragmentListener)this.Activity).onSuccessSelected(0);
        }
    }
}