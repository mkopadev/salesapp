using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using SalesApp.Core.ViewModels.Commissions;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.Commissions
{
    public class CommissionsInfoFragment : MvxFragment
    {
        public const string IconBundleKey = "IconBundleKey";
        public const string TitleBundleKey = "TitleBundleKey";
        public const string MessageBundleKey = "MessageBundleKey";

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            View view = this.BindingInflate(Resource.Layout.commissions_overlay_layout, null);

            bool showIcon = this.Arguments.GetBoolean(IconBundleKey);
            string message = this.Arguments.GetString(MessageBundleKey);
            string title = this.Arguments.GetString(TitleBundleKey);

            if (string.IsNullOrEmpty(title) && !showIcon)
            {
                TextView messageTextView = view.FindViewById<TextView>(Resource.Id.status_message);
                RelativeLayout.LayoutParams layoutParams = messageTextView.LayoutParameters as RelativeLayout.LayoutParams;

                if (layoutParams != null)
                {
                    layoutParams.AddRule(LayoutRules.CenterInParent);
                    messageTextView.LayoutParameters = layoutParams;
                }
            }

            InfoViewModel vm = new InfoViewModel
            {
                HasIcon = showIcon,
                Message = message,
                Title = title
            };

            this.ViewModel = vm;
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Commissions Information");

            return view;
        }
    }
}