using Android.App;
using MvvmCross.Droid.Support.V4;

namespace SalesApp.Droid.Views.Commissions
{
    public abstract class CommissionsFragmentBase : MvxFragment
    {
        protected CommissionsView parentActivity;

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this.parentActivity = (CommissionsView)activity;
        }
    }
}