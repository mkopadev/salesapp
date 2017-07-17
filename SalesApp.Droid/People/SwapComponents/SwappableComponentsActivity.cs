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
using SalesApp.Core.Logging;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.People.SwapComponents.Models;

namespace SalesApp.Droid.People.SwapComponents
{
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ProductComponentsActivity))]
    class SwappableComponentsActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(SwapFailedActivity));
        private RecyclerView recyclerView;
        private RecyclerView.Adapter adapter;
        private RecyclerView.LayoutManager layoutManager;
        private List<ProductComponent> _swappableProductComponentsList;
        private TextView _productTextView, _instructionsTextView, _noComponentsTextView, _componentsTitleTextView;
        private Button previousButton;
        private SwapProduct _product;
        private ProductComponent _selectedProductComponent;
        private ProductComponentsResponse _swappableProductComponentsResponse, _productComponentsResponse;
        private CustomerDetailsResponse _customerDetailsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _selectedProductComponent = JsonConvert.DeserializeObject<ProductComponent>(extras.GetString("productComponent"));
            _swappableProductComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("swappableProductComponentsResponse"));

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
            SetContentView(Resource.Layout.layout_swappable_components);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            //init
            _swappableProductComponentsList = new List<ProductComponent>();

            _productTextView = FindViewById<TextView>(Resource.Id.textView_product_name);
            _instructionsTextView = FindViewById<TextView>(Resource.Id.textView_instructions);
            _componentsTitleTextView = FindViewById<TextView>(Resource.Id.textView_components_title);
            _noComponentsTextView = FindViewById<TextView>(Resource.Id.textView_no_components);
            previousButton = FindViewById<Button>(Resource.Id.button_previous);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView_products);

            UpdateScreen();

            layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.AddItemDecoration(new SpaceItemDecoration(this, Resource.Dimension.recycler_items_space, true, true));
            adapter = new SwappableComponentsAdapter(_swappableProductComponentsResponse, this,
                _selectedProductComponent, _customerDetailsResponse, _product, _productComponentsResponse);
            recyclerView.SetAdapter(adapter);

            SetListeners();
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            if (_swappableProductComponentsResponse != null)
            {
                if (_swappableProductComponentsResponse.ProductComponents != null)
                {
                    _swappableProductComponentsList = _swappableProductComponentsResponse.ProductComponents;
                    if (_swappableProductComponentsList.Count == 0)
                    {
                        showNoComponentsMessage();
                    }
                }
                else
                {
                    _swappableProductComponentsList = new List<ProductComponent>();
                    showNoComponentsMessage();
                }
                _productTextView.Text = GetString(Resource.String.product_) + _product.ProductName;
                _componentsTitleTextView.Text = _selectedProductComponent.StockItem + " " + GetString(Resource.String.swap);
            }
            else
            {
                showNoComponentsMessage();
            }

        }

        private void showNoComponentsMessage(string customMessage = null)
        {
            _productTextView.Visibility = ViewStates.Gone;
            _instructionsTextView.Visibility = ViewStates.Gone;
            _componentsTitleTextView.Visibility = ViewStates.Gone;
            recyclerView.Visibility = ViewStates.Gone;
            string message = GetString(Resource.String.no_components);
            if (customMessage != null)
            {
                message = customMessage;
            }
            _noComponentsTextView.Visibility = ViewStates.Visible;
            _noComponentsTextView.Text = message;
            previousButton.Text = GetString(Resource.String.select_another_product);
        }

        public override void SetListeners()
        {
            previousButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(ProductComponentsActivity));
                Bundle extras = new Bundle();
                extras.PutString("product", JsonConvert.SerializeObject(_product));
                extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                intent.PutExtras(extras);
                StartActivity(intent);
            };

            //recyclerView.AddOnScrollListener(new MScrollListener(this));
        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            Intent intent = new Intent(this, typeof(ProductComponentsActivity));
            Bundle extras = new Bundle();
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
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