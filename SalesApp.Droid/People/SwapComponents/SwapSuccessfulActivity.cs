using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ConfirmSwapActivity))]
    //[Activity(Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    class SwapSuccessfulActivity : ActivityBase2
    {
        //private Toolbar toolbar;
        private Button swapAnotherButton, homeButton;
        private SwapProduct _product;
        private CustomerDetailsResponse _customerDetailsResponse;
        private ProductComponentsResponse _productComponentsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _product = JsonConvert.DeserializeObject<SwapProduct>(extras.GetString("product"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            _productComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("productComponentsResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_swap_success);
            // set the toolbar as actionbar
            //this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            //this.SetSupportActionBar(this.toolbar);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            swapAnotherButton = FindViewById<Button>(Resource.Id.button_swap_another);
            homeButton = FindViewById<Button>(Resource.Id.button_home);

            SetListeners();
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
            swapAnotherButton.Click += delegate
            {
                SwapAnother();
            };

            homeButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(HomeView));
                StartActivity(intent);
            };

        }

        private void SwapAnother()
        {
            Intent intent = new Intent(this, typeof(ProductComponentsActivity));
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
            intent.PutExtras(extras);
            StartActivity(intent);
        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            SwapAnother();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Intent intent = new Intent(this, typeof(HomeView));
                    StartActivity(intent);
                    break;
            }
            return true;
        }
    }
}