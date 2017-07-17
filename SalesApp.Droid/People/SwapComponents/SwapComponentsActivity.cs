using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(HomeView))]
    //[Activity(Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    class SwapComponentsActivity : ActivityBase2
    {
        public const int phoneTab = 0;
        public const int idTab = 1;
        public const int serialTab = 2;
        private SwapFragmentHolder[] fragments;
        private SwipeControlledViewPager pager;
        private ClickControlledTabLayout tabLayout;
        //        public static CustomerDetailsResponse customerDetailsResponse;
        //        public static ProductComponentsResponse productComponentsResponse;
        //        public static string identifier;
        //        public static int identifierType;
        private string _identifier = "";
        private int _identifierType = -1;
        //private Toolbar toolbar;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                if (extras.ContainsKey("identifierType"))
                {
                    _identifierType = extras.GetInt("identifierType");
                }
                if (extras.ContainsKey("identifier"))
                {
                    _identifier = extras.GetString("identifier");
                }
            }
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        /// <summary>
        /// Initialize the screen
        /// </summary>
        public override void InitializeScreen()
        {
            this.SetContentView(Resource.Layout.layout_swap_components);

            // set the toolbar as actionbar
            // this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            // this.SetSupportActionBar(this.toolbar);
            this.SetScreenTitle(this.GetString(Resource.String.component_swap));
            this.ActionBar.SetDisplayHomeAsUpEnabled(true);

            var phoneTabFragment = new CustomerIdentificationFragment();
            var phoneTabFragmentBundle = new Bundle();
            phoneTabFragmentBundle.PutInt(CustomerIdentificationFragment.TabBundleKey, phoneTab);
            if (!string.IsNullOrEmpty(_identifier))
            {
                phoneTabFragmentBundle.PutString("identifier", _identifier);
            }
            phoneTabFragment.Arguments = phoneTabFragmentBundle;

            var idTabFragment = new CustomerIdentificationFragment();
            var idTabFragmentBundle = new Bundle();
            idTabFragmentBundle.PutInt(CustomerIdentificationFragment.TabBundleKey, idTab);
            if (!string.IsNullOrEmpty(_identifier))
            {
                idTabFragmentBundle.PutString("identifier", _identifier);
            }
            idTabFragment.Arguments = idTabFragmentBundle;

            var serialTabFragment = new CustomerIdentificationFragment();
            var serialTabFragmentBundle = new Bundle();
            serialTabFragmentBundle.PutInt(CustomerIdentificationFragment.TabBundleKey, serialTab);
            if (!string.IsNullOrEmpty(_identifier))
            {
                serialTabFragmentBundle.PutString("identifier", _identifier);
            }
            serialTabFragment.Arguments = serialTabFragmentBundle;

            this.fragments = new[]
            {
                new SwapFragmentHolder
                {
                    TabId = phoneTab,
                    Fragment = phoneTabFragment,
                    Label = this.GetString(Resource.String.phone_number)
                    // Label = "Phone",
                },
                new SwapFragmentHolder
                {
                    TabId = idTab,
                    Fragment = idTabFragment,
                    Label = this.GetString(Resource.String.id_number)
                    // Label = "ID",
                },
                new SwapFragmentHolder
                {
                    TabId = serialTab,
                    Fragment = serialTabFragment,
                    Label = this.GetString(Resource.String.device)
                    // Label = "Device",
                }
            };

            // load pager
            this.pager = this.FindViewById<SwipeControlledViewPager>(Resource.Id.pager);
            this.pager.OffscreenPageLimit = this.fragments.Length - 1;
            this.pager.Adapter = new SwapFragmentsAdapter(this.GetFragmentManager(), this.fragments);
            this.pager.OffscreenPageLimit = this.fragments.Length - 1;

            // Give the TabLayout the ViewPager
            this.tabLayout = this.FindViewById<ClickControlledTabLayout>(Resource.Id.sliding_tabs);
            this.tabLayout.SetupWithViewPager(this.pager);
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {

        }

        public override void SetListeners()
        {

        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            Intent intent = new Intent(this, typeof(HomeView));
            StartActivity(intent);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }
            return true;
        }


        public class SwapFragmentHolder
        {
            public string Label { get; set; }

            public int TabId { get; set; }

            public FragmentBase3 Fragment { get; set; }
        }
    }
}