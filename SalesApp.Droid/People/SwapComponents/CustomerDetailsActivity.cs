using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Logging;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.People.SwapComponents.Models;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(SwapComponentsActivity))]
    class CustomerDetailsActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(ProductsAdapter));
        private RecyclerView recyclerView;
        private ProductsAdapter adapter;
        private RecyclerView.LayoutManager layoutManager;
        private List<SwapProduct> productsList;
        private TextView nameTextView, idNumberTextView, phoneNumberTextView;
        private Button previousButton;
        private string name, idNumber, phoneNumber;
        private CustomerDetailsResponse _customerDetailsResponse;
        private SwapComponentApi api;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_customer_details);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            //init
            productsList = new List<SwapProduct>();

            nameTextView = FindViewById<TextView>(Resource.Id.textView_customer_name);
            idNumberTextView = FindViewById<TextView>(Resource.Id.textView_idnumber);
            phoneNumberTextView = FindViewById<TextView>(Resource.Id.textView_customer_phone);
            previousButton = FindViewById<Button>(Resource.Id.button_previous);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView_products);

            UpdateScreen();

            layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            adapter = new ProductsAdapter(productsList, this);
            adapter.ItemClick += OnItemClick;
            recyclerView.SetAdapter(adapter);

            SetListeners();
        }

        void OnItemClick(object sender, int position)
        {
            SwapProduct product = productsList[position];
            //            //check if a product warranty is expired
            //            if (WarrantyIsExpired(product.WarrantyExpiryDate))
            //            {
            //                Intent intent = null;
            //                //show that the warranty is expired
            //                intent = new Intent(this, typeof(WarrantyExpiredActivity));
            //                //pass the product details
            //                Bundle extras = new Bundle();
            //                extras.PutString("productName", product.ProductName);
            //                intent.PutExtras(extras);
            //                //start activity
            //                StartActivity(intent);
            //            }
            //            else
            //            {
            //show the product components
            //intent = new Intent(context, typeof(ProductComponentsActivity));
            GetProductComponents(product);
            //            }
        }

        private async void GetProductComponents(SwapProduct product)
        {
            Log.Verbose("Get components of a product.");
            var progressDialog = ProgressDialog.Show(this, GetString(Resource.String.please_wait), GetString(Resource.String.loading_product_components), true);
            api = new SwapComponentApi();

            if (product.ProductName.Contains("Product III"))
            {
                product.ProductName = "Product III";
            }
            string parameters = "/components?ProductName=" + product.ProductName;
            ProductComponentsResponse productComponentsResponse = await api.GetProductDetails(Uri.EscapeUriString(parameters),
                filterFlags: ErrorFilterFlags.AllowEmptyResponses);
            progressDialog.Hide();
            if (productComponentsResponse.Successful)
            {
                Log.Verbose("API Call successful");
                //                SwapComponentsActivity.productComponentsResponse = productComponentsResponse;
                Intent intent = new Intent(this, typeof(ProductComponentsActivity));
                //pass the product details
                Bundle extras = new Bundle();
                extras.PutString("product", JsonConvert.SerializeObject(product));
                extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(productComponentsResponse));
                extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                intent.PutExtras(extras);
                StartActivity(intent);
            }
            else
            {
                Log.Verbose("Something went wrong");
                if (productComponentsResponse.ResponseText.Equals("not_connected"))
                {
                    Toast.MakeText(this, GetString(Resource.String.not_connected), ToastLength.Long).Show();
                }
            }
            //productComponentsResponse = CreateDummyResponse();
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            //            if (SwapComponentsActivity.customerDetailsResponse != null)
            //            {
            //            customerDetailsResponse = SwapComponentsActivity.customerDetailsResponse;
            name = _customerDetailsResponse.Surname + " " + _customerDetailsResponse.OtherNames;
            phoneNumber = _customerDetailsResponse.PhoneNumber;
            productsList = _customerDetailsResponse.Products;

            idNumber = _customerDetailsResponse.IdentificationNumber;
            nameTextView.Text = name;
            idNumberTextView.Text = idNumber;
            phoneNumberTextView.Text = phoneNumber;
            //        }
        }

        public override void SetListeners()
        {
            previousButton.Click += delegate
            {
                OnBackPressed();
            };
        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            Bundle extras = new Bundle();
            extras.PutInt("identifierType", _customerDetailsResponse.IdentifierType);
            extras.PutString("identifier", _customerDetailsResponse.Identifier);
            Intent intent = new Intent(this, typeof(SwapComponentsActivity));
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

        //return true if the expiry date is less than the current date
        public static bool WarrantyIsExpired(DateTime warrantyExpiryDate)
        {
            return DateTime.Now > warrantyExpiryDate;
        }
    }
}