using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Enums;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ConfirmSwapActivity))]
    //[Activity(Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    class SwapFailedActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(SwapFailedActivity));
        public const int MAX_FAILED_TRIALS = 3;
        //private Toolbar toolbar;
        private Button tryAgainButton, cancelButton;
        private string reason;
        //        private int stockId;
        private SwapComponentApi api;
        private int numOfRetries = 1;
        private ProgressDialog progressDialog;
        private bool maxRetriesReached;
        private TextView messageTextView;
        private ProductComponent _incomingProductComponent, _outgoingProductComponent;
        private SwapProduct _product;
        private CustomerDetailsResponse _customerDetailsResponse;
        private ProductComponentsResponse _productComponentsResponse;
        private SwapComponentResponse _swapComponentResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _incomingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("incomingProductComponent"));
            _outgoingProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("outgoingProductComponent"));
            reason = extras.GetString("reason");
            _product = JsonConvert.DeserializeObject<SwapProduct>(extras.GetString("product"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            _productComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("productComponentsResponse"));
            _swapComponentResponse = JsonConvert.DeserializeObject<SwapComponentResponse>(extras.GetString("swapComponentResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_swap_failed);
            // set the toolbar as actionbar
            //this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            //this.SetSupportActionBar(this.toolbar);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            tryAgainButton = FindViewById<Button>(Resource.Id.button_try_again);
            cancelButton = FindViewById<Button>(Resource.Id.button_cancel);
            messageTextView = FindViewById<TextView>(Resource.Id.textView_failed_message);

            if (_swapComponentResponse != null)
            {
                if (_swapComponentResponse.Status != ServiceReturnStatus.NoInternet)
                {
                    messageTextView.Text = _swapComponentResponse.Message;
                }
            }

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
            api = new SwapComponentApi();
            tryAgainButton.Click += delegate
            {
                if (!maxRetriesReached)
                {
                    TryAgain();
                }
                else
                {
                    SwapAnother();
                }
            };

            cancelButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(HomeView));
                StartActivity(intent);
            };

        }

        private void SwapAnother()
        {
            Intent intent = new Intent(this, typeof(ProductComponentsActivity));
            //pass the product details
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
            intent.PutExtras(extras);
            StartActivity(intent);
        }

        private void TryAgain()
        {
            Log.Verbose("Retries " + numOfRetries);
            if (numOfRetries < MAX_FAILED_TRIALS)
            {
                progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.swap_component_), true);
                SwapComponent();
            }
            else
            {
                progressDialog.Hide();
                maxRetriesReached = true;
                tryAgainButton.Text = GetString(Resource.String.swap_another_product);
                cancelButton.Text = GetString(Resource.String.return_to_home);
                messageTextView.Text = GetString(Resource.String.sending_failed_thrice) + " " + Settings.Instance.DealerSupportLine;
            }
        }

        private async void SwapComponent()
        {
            Log.Verbose("Trying to Swap Component");
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
            SwapComponentResponse swapComponentResponse = await api.SwapComponent(request);
            if (swapComponentResponse.Success)
            {
                Log.Verbose("Swap was successful");
                Bundle extras = new Bundle();
                extras.PutString("product", JsonConvert.SerializeObject(_product));
                extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                Intent intent = new Intent(this, typeof(SwapSuccessfulActivity));
                //pass the product details
                intent.PutExtras(extras);
                StartActivity(intent);
            }
            else
            {
                Log.Verbose("Something went wrong - Swap failed");
                numOfRetries = numOfRetries + 1;
                //TryAgain();
                progressDialog.Hide();
            }
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
                    //OnBackPressed();
                    Intent intent = new Intent(this, typeof(HomeView));
                    StartActivity(intent);
                    break;
            }
            return true;
        }
    }
}