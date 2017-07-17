using Android.App;
using MvvmCross.Droid.Support.V4;

namespace SalesApp.Droid.Views.Modules
{
    public class ModuleFragmentBase : MvxFragment
    {
        protected IFragmentLoadStateListener FragmentLoadStateListener;

        public override void OnResume()
        {
            base.OnResume();
            this.FragmentLoadStateListener.IndicatorStateChanged(false);
            this.FragmentLoadStateListener.CanRegisterChanged(true);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this.FragmentLoadStateListener = activity as IFragmentLoadStateListener;
        }
    }
}