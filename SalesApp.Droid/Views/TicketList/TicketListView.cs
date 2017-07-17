using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.UI.Stats;
using SalesApp.Droid.Views.TicketList.Customer;
using SalesApp.Droid.Views.TicketList.Dsr;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.TicketList
{
    /// <summary>
    /// This is the activity that holds both customer and DSR tickets' fragments
    /// </summary>
    [Activity(Label = "@string/ticket_list_screen_title", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(HomeView), Theme = "@style/AppTheme.SmallToolbar")]
    public class TicketListView : ActivityWithNavigation
    {
        /// <summary>
        /// Represents the customer tickets fragment, the first fragment
        /// </summary>
        public const int CustomerTicketsPage = 0;

        /// <summary>
        /// Represents the DSR tickets fragment, the second fragment
        /// </summary>
        public const int DsrTicketsPage = 1;

        /// <summary>
        /// The current page/fragment that we are viewing
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// An array of the attached fragments
        /// </summary>
        private SparseArray<Fragment> fragments;

        /// <summary>
        /// The tabs
        /// </summary>
        private ClickControlledTabLayout tabLayout;

        /// <summary>
        /// The swipe pager for the tabs
        /// </summary>
        private SwipeControlledViewPager pager;

        /// <summary>
        /// The swipe refresh layout
        /// </summary>
        private SwipeRefreshLayout swipeRefreshLayout;

        /// <summary>
        /// Gets the swipe refresh layout
        /// </summary>
        public SwipeRefreshLayout SwipeRefreshLayout
        {
            get
            {
                return this.swipeRefreshLayout;
            }
        }

        /// <summary>
        /// Initializes the activity
        /// </summary>
        public override void InitializeScreen()
        {
        }

        /// <summary>
        /// Retrieves user input into local variables
        /// </summary>
        public override void RetrieveScreenInput()
        {
        }

        /// <summary>
        /// Updates the screen
        /// </summary>
        public override void UpdateScreen()
        {
        }

        /// <summary>
        /// Sets the Listeners
        /// </summary>
        public override void SetListeners()
        {
        }

        /// <summary>
        /// Returns true if validation functions pass, false otherwise
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        public override bool Validate()
        {
            return true;
        }

        /// <summary>
        /// Sets the screen permissions
        /// </summary>
        public override void SetViewPermissions()
        {
        }

        /// <summary>
        /// Called when a fragment is attached to this activity
        /// </summary>
        /// <param name="fragment">The fragment that is being attached</param>
        public override void OnAttachFragment(Fragment fragment)
        {
            base.OnAttachFragment(fragment);
            if (this.fragments == null)
            {
                this.fragments = new SparseArray<Fragment>();
            }

            if (fragment.GetType() == typeof(CustomerTicketsFragment))
            {
                this.fragments.Put(CustomerTicketsPage, fragment);
            }

            if (fragment.GetType() == typeof(DsrTicketsFragment))
            {
                this.fragments.Put(DsrTicketsPage, fragment);
            }
        }

        /// <summary>
        /// Called by the android framework when this activity is being created for the first time
        /// </summary>
        /// <param name="savedInstanceState">Saved state if any</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.layout_ticket_list);

            this.AddToolbar(Resource.String.ticket_list_screen_title, true);

            // load pager
            this.pager = this.FindViewById<SwipeControlledViewPager>(Resource.Id.pager);
            this.pager.Adapter = new TicketListPagerAdapter(this.GetFragmentManager(), this);
            this.pager.PageSelected += this.PageSelectedEventHandler;

            // Give the TabLayout the ViewPager
            this.tabLayout = this.FindViewById<ClickControlledTabLayout>(Resource.Id.sliding_tabs);
            this.tabLayout.SetupWithViewPager(this.pager);

            this.swipeRefreshLayout = this.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            this.swipeRefreshLayout.Refresh += this.SwipeRefreshHandler;
            this.swipeRefreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.white, Resource.Color.gray1, Resource.Color.green);
        }

        /// <summary>
        /// Called when a refresh action is initiated via pull to sync
        /// </summary>
        /// <param name="sender">The event source. An instance of <see cref="SwipeRefreshLayout"/></param>
        /// <param name="e">The event args</param>
        protected async void SwipeRefreshHandler(object sender, EventArgs e)
        {
            this.swipeRefreshLayout.Refreshing = true;

            ISwipeRefreshFragment currentFragmnt = (ISwipeRefreshFragment)this.fragments.Get(this.CurrentPage);

            await currentFragmnt.SwipeRefresh();

            this.swipeRefreshLayout.Refreshing = false;
        }

        /// <summary>
        /// Called when a refresh action is initiated via pull to sync
        /// </summary>
        /// <param name="sender">The event source. An instance of <see cref="SwipeControlledViewPager"/></param>
        /// <param name="e">The page selection event args</param>
        protected void PageSelectedEventHandler(object sender, ViewPager.PageSelectedEventArgs e)
        {
            this.CurrentPage = e.Position;

            TicketFragmentBase currentFragment = (TicketFragmentBase)this.fragments.Get(this.CurrentPage);

            this.SwipeRefreshLayout.Enabled = currentFragment.CanPullToSync;

            if (currentFragment.TicketList.Adapter.Count == 0)
            {
                currentFragment.ShowSnackBar();
            }

            ISwipeRefreshFragment swipeRefreshFragment = (ISwipeRefreshFragment)this.fragments.Get(this.CurrentPage);
            swipeRefreshFragment.SwipeRefresh(false);
        }
    }
}