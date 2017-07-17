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
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(CustomerDetailsActivity))]
    class WarrantyExpiredActivity : ActivityBase2
    {
        private TextView nameTextView, idNumberTextView, phoneNumberTextView, warrantyMessageTextView;
        private Button homeButton, selectAnotherButton;
        private string name, idNumber, phoneNumber;
        private string warrantyMessage;
        private CustomerDetailsResponse _customerDetailsResponse;
        private SwapProduct _product;
        private ProductComponent _productComponent;
        private ProductComponentsResponse _productComponentsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _product = JsonConvert.DeserializeObject<SwapProduct>(extras.GetString("product"));
            _productComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("productComponent"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            _productComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("productComponentsResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_warranty_expired);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            nameTextView = FindViewById<TextView>(Resource.Id.textView_customer_name);
            idNumberTextView = FindViewById<TextView>(Resource.Id.textView_idnumber);
            phoneNumberTextView = FindViewById<TextView>(Resource.Id.textView_customer_phone);
            warrantyMessageTextView = FindViewById<TextView>(Resource.Id.textView_warranty_message);
            homeButton = FindViewById<Button>(Resource.Id.button_home);
            selectAnotherButton = FindViewById<Button>(Resource.Id.button_swap_another);

            UpdateScreen();

            SetListeners();
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            //            if (SwapComponentsActivity.customerDetailsResponse != null)
            //            {
            //                customerDetailsResponse = SwapComponentsActivity.customerDetailsResponse;
            name = _customerDetailsResponse.Surname + " " + _customerDetailsResponse.OtherNames;
            idNumber = _customerDetailsResponse.IdentificationNumber;
            phoneNumber = _customerDetailsResponse.PhoneNumber;
            warrantyMessage = string.Format(GetString(Resource.String.warranty_expired_no_swap), _productComponent.StockItem);

            nameTextView.Text = name;
            idNumberTextView.Text = idNumber;
            phoneNumberTextView.Text = phoneNumber;
            warrantyMessageTextView.Text = warrantyMessage;
            //            }
        }

        public override void SetListeners()
        {
            homeButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(HomeView));
                StartActivity(intent);
            };

            selectAnotherButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(ProductComponentsActivity));
                Bundle extras = new Bundle();
                extras.PutString("product", JsonConvert.SerializeObject(_product));
                extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                intent.PutExtras(extras);
                StartActivity(intent);
            };
        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    //OnBackPressed();
                    Intent intent = new Intent(this, typeof(HomeView));
                    StartActivity(intent);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}