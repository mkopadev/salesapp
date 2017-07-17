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
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ReasonForSwapActivity))]
    //[Activity(Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    class ConfirmSwapActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(ConfirmSwapActivity));
        //private Toolbar toolbar;
        //        private int stockId;
        //        private string componentName, productName;
        private TextView _customerNameTextView, _productTextView, _componentSwappedTextView, _componentSwappedWithTextView;
        private Button previousButton, nextButton;
        private string customerName, reason;
        private TextView reasonTextView;
        private SwapComponentApi api;
        private TextView errorTextView;
        private ProductComponent _incomingProductComponent, _outgoingProductComponent;
        private SwapProduct _product;
        private CustomerDetailsResponse _customerDetailsResponse;
        private ProductComponentsResponse _productComponentsResponse;
        private ProductComponent _selectedProductComponent;
        private ProductComponentsResponse _swappableProductComponentsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _incomingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("incomingProductComponent"));
            _outgoingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("outgoingProductComponent"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            reason = extras.GetString("reason");
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
            SetContentView(Resource.Layout.layout_swap_confirmation);
            // set the toolbar as actionbar
            //this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            //this.SetSupportActionBar(this.toolbar);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            _customerNameTextView = FindViewById<TextView>(Resource.Id.textView_customer_name);
            _productTextView = FindViewById<TextView>(Resource.Id.textView_product);
            _componentSwappedTextView = FindViewById<TextView>(Resource.Id.textView_component_swapped);
            _componentSwappedWithTextView = FindViewById<TextView>(Resource.Id.textView_component_swapped_with);
            reasonTextView = FindViewById<TextView>(Resource.Id.textView_reason);
            previousButton = FindViewById<Button>(Resource.Id.button_previous);
            nextButton = FindViewById<Button>(Resource.Id.button_next);
            errorTextView = FindViewById<TextView>(Resource.Id.textViewError);

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
            //                _customerDetailsResponse = SwapComponentsActivity.customerDetailsResponse;
            customerName = _customerDetailsResponse.Surname + " " + _customerDetailsResponse.OtherNames;
            _customerNameTextView.Text = customerName;
            _productTextView.Text = GetString(Resource.String.product_) + " " + _incomingProductComponent.Product;
            _componentSwappedTextView.Text = GetString(Resource.String.component_swapped_) + " " + _incomingProductComponent.StockItem;
            _componentSwappedWithTextView.Text = GetString(Resource.String.swapped_with_) + " " + _outgoingProductComponent.Product + " " + _outgoingProductComponent.StockItem;
            reasonTextView.Text = reason;
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
                SwapComponent();
            };

        }

        private async void SwapComponent()
        {
            SwapComponentRequest request = new SwapComponentRequest
            {
                RequestType = _customerDetailsResponse.IdentifierType,
                RequestValue = _customerDetailsResponse.Identifier,
                IncomingComponentId = _incomingProductComponent.StockId,
                OutgoingComponentId = _outgoingProductComponent.StockId,
                Reason = reason,
            };

            Log.Verbose("Swap details RequestType= " + request.RequestType + ",RequestValue= " + request.RequestValue
                + " Incoming ComponentId= " + request.IncomingComponentId + " Outgoing ComponentId= " + request.OutgoingComponentId
                + ",Reason " + request.Reason);
            Log.Verbose("Get components of a product.");
            var progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.swapping_component), true);
            api = new SwapComponentApi();
            SwapComponentResponse swapComponentResponse = await api.SwapComponent(request);
            progressDialog.Hide();

            Intent intent = null;
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
            if (swapComponentResponse.Success)
            {
                Log.Verbose("Swap was successful");
                intent = new Intent(this, typeof(SwapSuccessfulActivity));
            }
            else
            {
                Log.Verbose("Something went wrong - Swap failed");
                intent = new Intent(this, typeof(SwapFailedActivity));
                //                extras.PutInt("stockId", _incomingProductComponent.StockId);
                //                extras.PutString("reason", reason);
                extras.PutString("incomingProductComponent", JsonConvert.SerializeObject(_incomingProductComponent));
                extras.PutString("outgoingProductComponent", JsonConvert.SerializeObject(_outgoingProductComponent));
                extras.PutString("reason", reason);
                extras.PutString("swapComponentResponse", JsonConvert.SerializeObject(swapComponentResponse));
            }
            //pass the product details
            intent.PutExtras(extras);
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("incomingProductComponent", JsonConvert.SerializeObject(_incomingProductComponent));
            extras.PutString("outgoingProductComponent", JsonConvert.SerializeObject(_outgoingProductComponent));

            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));

            extras.PutString("productComponent", JsonConvert.SerializeObject(_selectedProductComponent));
            extras.PutString("swappableProductComponentsResponse", JsonConvert.SerializeObject(_swappableProductComponentsResponse));
            Intent intent = new Intent(this, typeof(ReasonForSwapActivity));
            //pass the details
            intent.PutExtras(extras);
            StartActivity(intent);
        }

        public override bool Validate()
        {
            return true;
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