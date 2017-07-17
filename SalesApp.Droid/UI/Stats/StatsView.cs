using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using SalesApp.Core.Auth;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels.Stats;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Framework;
using SalesApp.Droid.UI.SwipableViews;

namespace SalesApp.Droid.UI.Stats
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StatsView : MvxNavigationViewBase<StatsViewModel>, ISwipeRefreshable
    {
        public const int SalesTab = 0;
        public const int RankingTab = 1;
        public const int UnitsTab = 2;

        private SwipeRefreshLayout _swipeRefreshLayout;
        private ISalesAppSession session;
        private SwipeControlledViewPager _pager;
        private ClickControlledTabLayout _tabLayout;
        private Toolbar _toolbar;
        private int[] _scrollPositions = { 0, 0, 0 };
        private int _previousTab;

        private string GetFragmentTag(int viewPagerId, int fragmentPosition)
        {
            return "android:switcher:" + viewPagerId + ":" + fragmentPosition;
        }

        public void ScrollYChanged(ScrollInformation scrollInformation)
        {
            if (scrollInformation.Y > 0 && _swipeRefreshLayout.Enabled)
            {
                this._swipeRefreshLayout.Enabled = false;
            }

            // if the user is at the top of the screen, enable SwipeRefreshLayout
            if (scrollInformation.Y == 0 && !_swipeRefreshLayout.Enabled)
            {
                this._swipeRefreshLayout.Enabled = true;
            }

            this._scrollPositions[this._pager.CurrentItem] = scrollInformation.Y;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.session = Resolver.Instance.Get<ISalesAppSession>();

            this.SetContentView(Resource.Layout.layout_stats_main);

            // set the toolbar as actionbar
            this._toolbar = this.FindViewById<Toolbar>(Resource.Id.main_toolbar);
            this.SetSupportActionBar(this._toolbar);

            string title = string.Format(
                this.GetString(Resource.String.stats_screen_title), this.session.FirstName, this.session.LastName);

            this.SetScreenTitle(title);

            // load pager
            this._pager = this.FindViewById<SwipeControlledViewPager>(Resource.Id.pager);
            this._pager.Adapter = new StatsFragmentAdapter(this.GetFragmentManager(), this);

            // Give the TabLayout the ViewPager
            this._tabLayout = this.FindViewById<ClickControlledTabLayout>(Resource.Id.sliding_tabs);
            this._tabLayout.SetupWithViewPager(this._pager);

            this._swipeRefreshLayout = this.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            this._swipeRefreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.white, Resource.Color.gray1, Resource.Color.green);
            this._swipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
            this._pager.PageSelected += this.PagerOnPageSelected;
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs eventArgs)
        {
            var frag = this.SupportFragmentManager.FindFragmentByTag(GetFragmentTag(Resource.Id.pager, this._pager.CurrentItem)) as ISwipeRefreshFragment;

            if (frag == null)
            {
                return;
            }

            frag.SwipeRefresh();
        }

        private void PagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            var frag = this.SupportFragmentManager.FindFragmentByTag(GetFragmentTag(Resource.Id.pager, this._pager.CurrentItem)) as ISwipeRefreshFragment;

            if (frag == null)
            {
                return;
            }
            
            if (Math.Abs(this._pager.CurrentItem - this._previousTab) > this._pager.OffscreenPageLimit)
            {
                this._scrollPositions[this._pager.CurrentItem] = 0;
            }

            this._swipeRefreshLayout.Enabled = this._scrollPositions[this._pager.CurrentItem] == 0;

            frag.SwipeRefresh(false);
            this._previousTab = this._pager.CurrentItem;
        }

        public void SetIsBusy(bool busy)
        {
            this._swipeRefreshLayout.Refreshing = busy;
            this._pager.AllowSwipe = !busy;
            this._tabLayout.CanClick = !busy;
        }
    }
}