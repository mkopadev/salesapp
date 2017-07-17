using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.ViewModels.Person.Customer;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Framework;
using SalesApp.Droid.UI.Stats;
using SalesApp.Droid.UI.Utils;
using SalesApp.Droid.Views.TicketList.Customer;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.People.Customers
{
    [Activity(NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(CustomerListView), Theme = "@style/AppTheme.SmallToolbar")]
    public class CustomerDetailView : MvxViewBase<CustomerDetailViewModel>, CustomerDetailFragment.IRegistrationStatusListener
    {
        /// <summary>
        /// The key for customer JSON in the recreation bundle
        /// </summary>
        public const string BundledCustomer = "Customer";

        private RegistrationFinishedFragmentAlert _registrationFailedFragmentAlert;

        /// <summary>
        /// The customer search result
        /// </summary>
        private CustomerSearchResult _customerSearchResult;

        /// <summary>
        /// The swipe pager for the tabs
        /// </summary>
        private SwipeControlledViewPager _pager;

        /// <summary>
        /// The tabs
        /// </summary>
        private ClickControlledTabLayout _tabLayout;

        /// <summary>
        /// An array of the attached fragments
        /// </summary>
        private SparseArray<Fragment> _fragments;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.layout_phone);
            this.AddToolbar(Resource.String.customer_details, true);

            Bundle extras = this.Intent.Extras;
            if (extras != null)
            {
                string bundledCustomerString = extras.GetString(BundledCustomer);
                this._customerSearchResult = JsonConvert.DeserializeObject<CustomerSearchResult>(bundledCustomerString);
                this.SetScreenTitle(this._customerSearchResult.FullName);
            }

            if (savedInstanceState != null)
            {
                string json = savedInstanceState.GetString(BundledCustomer);
                this._customerSearchResult = JsonConvert.DeserializeObject<CustomerSearchResult>(json);
                this.SetScreenTitle(this._customerSearchResult.FullName);
            }

            // load the pager
            this._pager = this.FindViewById<SwipeControlledViewPager>(Resource.Id.pager);
            var pagerAdapter = new CustomerDetailsPagerAdapter(this.GetFragmentManager(), this._customerSearchResult);
            this._pager.Adapter = pagerAdapter;
            this._pager.PageSelected += this.PageSelectedEventHandler;

            // Give the TabLayout the ViewPager
            this._tabLayout = this.FindViewById<ClickControlledTabLayout>(Resource.Id.sliding_tabs);
            this._tabLayout.SetupWithViewPager(this._pager);
        }

        /// <summary>
        /// Called when a refresh action is initiated via pull to sync
        /// </summary>
        /// <param name="sender">The event source. An instance of <see cref="SwipeControlledViewPager"/></param>
        /// <param name="e">The page selection event args</param>
        protected void PageSelectedEventHandler(object sender, ViewPager.PageSelectedEventArgs e)
        {
            ISwipeRefreshFragment swipeRefreshFragment = this._fragments.Get(this._pager.CurrentItem) as ISwipeRefreshFragment;

            if (swipeRefreshFragment == null)
            {
                return;
            }

            swipeRefreshFragment.SwipeRefresh(false);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(BundledCustomer, JsonConvert.SerializeObject(this._customerSearchResult));
        }

        public override void SetViewPermissions()
        {
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void OnRegistrationFinished(Bundle arguments)
        {
            this._registrationFailedFragmentAlert = new RegistrationFinishedFragmentAlert();
            this._registrationFailedFragmentAlert.Arguments = arguments;
            this._registrationFailedFragmentAlert.Show(GetFragmentManager(), "Customer");
        }

        public void OnRegistrationFinished(string arguments, string messageDetail)
        {
            ProgressDialogBuilder builder = new ProgressDialogBuilder();
            builder.SetText(null, GetString(Resource.String.registering) + messageDetail);
            builder.Show(this);
        }

        public void OnRegistrationCanceled()
        {
            if (this._registrationFailedFragmentAlert != null)
            {
                this._registrationFailedFragmentAlert.Dismiss();
            }
        }

        public override void OnAttachFragment(Fragment fragment)
        {
            base.OnAttachFragment(fragment);
            if (this._fragments == null)
            {
                this._fragments = new SparseArray<Fragment>();
            }

            if (fragment.GetType() == typeof(CustomerDetailFragment))
            {
                this._fragments.Put(CustomerDetailsPagerAdapter.CustomerRegistrationInfoFragment, fragment);
            }

            if (fragment.GetType() == typeof(CustomerTicketsFragment))
            {
                this._fragments.Put(CustomerDetailsPagerAdapter.CustomerTicketsFragmentPage, fragment);
            }

            if (fragment.GetType() == typeof(FragmentCustomerPhotos))
            {
                this._fragments.Put(CustomerDetailsPagerAdapter.CustomerPhotosFragmentPage, fragment);
            }
        }
    }
}