using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using SalesApp.Core.ViewModels.Stats.Reporting;
using SalesApp.Droid.Framework;
using SalesApp.Droid.UI.SwipableViews;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.UI.Stats.Reporting
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ReportingLevelStatsView : MvxNavigationViewBase<ReportingLevelStatsViewModel>, ISwipeRefreshable
    {
        private const string ReportingLevelFragmentTag = "ReportingLevelFragmentTag";
        private SwipeRefreshLayout _swipeRefreshLayout;
        private Fragment _reportingStatsFragment;

        public void SwipeRefreshLayoutOnRefresh(object sender, EventArgs eventArgs)
        {
            var frag = SupportFragmentManager.FindFragmentByTag(ReportingLevelFragmentTag) as ISwipeRefreshFragment;

            if (frag == null)
            {
                return;
            }

            frag.SwipeRefresh();
        }

        /// <summary>
        /// This method controls enabling and disabling of the SwipeRefreshLayout.
        /// </summary>
        /// <param name="scrollInformation">Scroll information from the screen</param>
        public void ScrollYChanged(ScrollInformation scrollInformation)
        {
            // Disable SwipeRefreshLayout when user scrolled down
            if (scrollInformation.Y > 0 && _swipeRefreshLayout.Enabled)
            {
                _swipeRefreshLayout.Enabled = false;
            }

            // if the user is at the top of the screen, enable SwipeRefreshLayout
            if (scrollInformation.Y == 0 && !_swipeRefreshLayout.Enabled)
            {
                _swipeRefreshLayout.Enabled = true;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_aggregated_reporting_activity);
            AddToolbar(Resource.String.reporting_screen_title, true);

            this._swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            this._swipeRefreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.white, Resource.Color.gray1, Resource.Color.green);

            // load fragment
            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
            var frag = SupportFragmentManager.FindFragmentByTag(ReportingLevelFragmentTag);
            if (frag == null)
            {
                this._reportingStatsFragment = new ReportingLevelStatsFragment();
                var bundle = new Bundle();
                this._reportingStatsFragment.Arguments = bundle;

                fragmentTransaction.Replace(Resource.Id.reporting_placeholder, this._reportingStatsFragment, ReportingLevelFragmentTag);
            }
            else
            {
                this._reportingStatsFragment = frag;
            }

            fragmentTransaction.Commit();

            _swipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
        }

        public void SetIsBusy(bool busy)
        {
            this._swipeRefreshLayout.Refreshing = busy;
        }
    }
}