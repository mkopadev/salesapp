using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Logging;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ProductComponentsActivity))]
    //[Activity(Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    class ReasonForSwapActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(ReasonForSwapActivity));
        //private Toolbar toolbar;
        //        private int id, _stockId, _productId;
        //        private string _componentName, _productName;
        private TextView _customerNameTextView, _productTextView, _componentSwappedTextView, _componentSwappedWithTextView;
        private Button previousButton, nextButton;
        private string customerName, reason;
        private EditText reasonEditText;
        private TextView errorTextView;
        private ProductComponent _incomingProductComponent, _outgoingProductComponent, _selectedProductComponent;
        private SwapProduct _product;
        private CustomerDetailsResponse _customerDetailsResponse;
        private ProductComponentsResponse _productComponentsResponse, _swappableProductComponentsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _incomingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("incomingProductComponent"));
            _outgoingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("outgoingProductComponent"));

            _product = JsonConvert.DeserializeObject<SwapProduct>(extras.GetString("product"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            _productComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("productComponentsResponse"));

            _selectedProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("productComponent"));
            _swappableProductComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("swappableProductComponentsResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_swap_reason);
            // set the toolbar as actionbar
            //this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            //this.SetSupportActionBar(this.toolbar);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            _customerNameTextView = FindViewById<TextView>(Resource.Id.textView_customer_name);
            _productTextView = FindViewById<TextView>(Resource.Id.textView_product);
            _componentSwappedTextView = FindViewById<TextView>(Resource.Id.textView_component_swapped);
            _componentSwappedWithTextView = FindViewById<TextView>(Resource.Id.textView_component_swapped_with);
            reasonEditText = FindViewById<EditText>(Resource.Id.editText_reason);
            previousButton = FindViewById<Button>(Resource.Id.button_previous);
            nextButton = FindViewById<Button>(Resource.Id.button_next);
            errorTextView = FindViewById<TextView>(Resource.Id.textViewError);
            reasonEditText.SetHintTextColor(Resources.GetColor(Resource.Color.grey_dark));

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
            customerName = _customerDetailsResponse.Surname + " " + _customerDetailsResponse.OtherNames;
            _customerNameTextView.Text = customerName;
            _productTextView.Text = GetString(Resource.String.product_) + " " + _incomingProductComponent.Product;
            _componentSwappedTextView.Text = GetString(Resource.String.component_swapped_) + " " + _incomingProductComponent.StockItem;
            _componentSwappedWithTextView.Text = GetString(Resource.String.swapped_with_) + " " + _outgoingProductComponent.Product + " " + _outgoingProductComponent.StockItem;
            //            }
        }

        public override void SetListeners()
        {
            previousButton.Click += delegate
            {
                OnBackPressed();
            };

            nextButton.Click += delegate
            {
                if (Validate())
                {
                    Bundle extras = new Bundle();
                    extras.PutString("incomingProductComponent", JsonConvert.SerializeObject(_incomingProductComponent));
                    extras.PutString("outgoingProductComponent", JsonConvert.SerializeObject(_outgoingProductComponent));
                    extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                    extras.PutString("reason", reason);
                    extras.PutString("product", JsonConvert.SerializeObject(_product));
                    extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                    extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                    extras.PutString("productComponent", JsonConvert.SerializeObject(_selectedProductComponent));
                    extras.PutString("swappableProductComponentsResponse", JsonConvert.SerializeObject(_swappableProductComponentsResponse));
                    Intent intent = new Intent(this, typeof(ConfirmSwapActivity));
                    //pass the details
                    intent.PutExtras(extras);
                    StartActivity(intent);
                    //                    extras.PutString("productName", productName);
                    //                    extras.PutInt("stockId", stockId);
                    //                    extras.PutString("componentName", componentName);
                }
            };

            nextButton.Enabled = false;
            reasonEditText.AfterTextChanged += (sender, args) =>
            {
                reason = reasonEditText.Text.ToString();
                if (!string.IsNullOrEmpty(reason))
                {
                    nextButton.Enabled = true;
                }
                else
                {
                    nextButton.Enabled = false;
                }
            };
        }

        public override bool Validate()
        {
            reason = reasonEditText.Text.ToString();
            if (!string.IsNullOrWhiteSpace(reason))
            {
                return true;
            }
            else
            {
                //reasonEditText.SetError("Required", null);
                errorTextView.Text = GetString(Resource.String.provide_swap_reason);
                errorTextView.Visibility = ViewStates.Visible;
                return false;
            }
        }

        public override void OnBackPressed()
        {
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
            extras.PutString("productComponent", JsonConvert.SerializeObject(_selectedProductComponent));
            extras.PutString("swappableProductComponentsResponse", JsonConvert.SerializeObject(_swappableProductComponentsResponse));
            Intent intent = new Intent(this, typeof(SwappableComponentsActivity));
            //pass the details
            intent.PutExtras(extras);
            StartActivity(intent);
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