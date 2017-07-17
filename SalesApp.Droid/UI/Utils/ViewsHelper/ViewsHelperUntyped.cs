using Android.Views;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.UI.Utils.ViewsHelper
{
    public class ViewsHelperUntyped : ViewsHelper<object>
    {
        public ViewsHelperUntyped(ActivityBase activity, View containerView)
            : base(activity, containerView)
        {
        }
    }
}