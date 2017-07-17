using System;
using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using SalesApp.Core.ViewModels.Commissions;
using SalesApp.Droid.Components.UIComponents.Layouts;
using SalesApp.Droid.Framework;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.Views.Commissions.Summary;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.Commissions
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CommissionsView : MvxNavigationViewBase<CommissionsViewModel>
    {
        private Toolbar _toolbar;
        public const string CommissionSummaryTag = "CommissionSummaryTag";
        public const string CommissionDetailsTag = "CommissionDetailsTag";

        /// <summary>
        /// The swipe refresh layout
        /// </summary>
        private SalesAppSwipeRefreshLayout _swipeRefreshLayout;

        public SalesAppSwipeRefreshLayout SwipeRefreshLayout
        {
            get { return this._swipeRefreshLayout; }
        }

        public DateTime CurrentMonth { get; set; }

        public int MonthDelta { get; set; }

        private bool _isShowingDetails;

        protected override void OnCreate(Bundle savedState)
        {
            base.OnCreate(savedState);
            SetContentView(Resource.Layout.layout_individual_commissions);

            this._swipeRefreshLayout = this.FindViewById<SalesAppSwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            this._swipeRefreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.white, Resource.Color.gray1, Resource.Color.green);
            this._swipeRefreshLayout.Refresh += this.SwipeRefreshHandler;

            AddToolbar(Resource.String.commissions, true);
            this.CurrentMonth = DateTime.Now;

            if (savedState != null)
            {
                string dateString = savedState.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);
                this.MonthDelta = savedState.GetInt(FragmentCommissionSummary.MonthDeltaBundleKey);

                this.CurrentMonth = DateTime.Parse(dateString);
                
                if (this.CurrentMonth == default(DateTime))
                {
                    this.CurrentMonth = DateTime.Now;
                }
            }

            this.ViewModel.PrevAction = this.LoadSummaryFragment;
            Fragment detailsFragment = SupportFragmentManager.FindFragmentByTag(CommissionDetailsTag);
            if (detailsFragment == null)
            {
                this.LoadSummaryFragment();
            }
            else
            {
                this.LoadFragment(detailsFragment, CommissionDetailsTag);
            }
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Commissions View");
        }

        /// <summary>
        /// Called when a refresh action is initiated via pull to sync
        /// </summary>
        /// <param name="sender">The event source. An instance of <see cref="SwipeRefreshLayout"/></param>
        /// <param name="e">The event args</param>
        protected async void SwipeRefreshHandler(object sender, EventArgs e)
        {
            this._swipeRefreshLayout.Refreshing = true;

            var fragment = (FragmentCommissionSummary) SupportFragmentManager.FindFragmentByTag(CommissionSummaryTag);

            if (fragment != null)
            {
                await fragment.LoadData();
            }

            this._swipeRefreshLayout.Refreshing = false;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString(FragmentCommissionSummary.CurrentMonthBundleKey, this.CurrentMonth.ToString(CultureInfo.CurrentCulture));
            outState.PutInt(FragmentCommissionSummary.MonthDeltaBundleKey, this.MonthDelta);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            bool showDetails = !this.MenuDrawerToggle.DrawerIndicatorEnabled;
            if (item.ItemId == Android.Resource.Id.Home && showDetails)
            {
                this.LoadSummaryFragment();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (this._isShowingDetails)
            {
                // load the summary page
                this.LoadSummaryFragment();

                return;
            }

            base.OnBackPressed();
        }

        private void LoadSummaryFragment()
        {
           var fragment = (FragmentCommissionSummary)SupportFragmentManager.FindFragmentByTag(CommissionSummaryTag);

            if (fragment == null)
            {
                fragment = new FragmentCommissionSummary();
                Bundle bundle = new Bundle();
                bundle.PutString(FragmentCommissionSummary.CurrentMonthBundleKey, CurrentMonth.ToString(CultureInfo.CurrentCulture));
                bundle.PutInt(FragmentCommissionSummary.MonthDeltaBundleKey, MonthDelta);
                fragment.Arguments = bundle;
            }

            LoadFragment(fragment, CommissionSummaryTag);
            this.SetScreenTitle(Resource.String.commissions);
            _isShowingDetails = false;
        }

        public void LoadFragment(Fragment fragment, string tag)
        {
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.main_content, fragment, tag)
                        .Commit();

            this._isShowingDetails = true;
        }
    }
}