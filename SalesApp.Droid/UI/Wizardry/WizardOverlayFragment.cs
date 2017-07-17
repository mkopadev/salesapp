using Android.OS;
using Android.Views;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.UI.Wizardry
{
    public abstract class WizardOverlayFragment : FragmentBase3
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container != null)
            {
                container.Clickable = true;
                container.Click += (sender, args) => { };
                container.Touch += (sender, args) => { };
            }

            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}