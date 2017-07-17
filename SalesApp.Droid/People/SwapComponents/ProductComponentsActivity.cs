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
    [Activity(Theme = "@style/AppTheme.Compat", ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(CustomerDetailsActivity))]
    class ProductComponentsActivity : ActivityBase2
    {
        private static readonly ILog Log = LogManager.Get(typeof(SwapFailedActivity));
        private RecyclerView recyclerView;
        private RecyclerView.Adapter adapter;
        private RecyclerView.LayoutManager layoutManager;
        private List<ProductComponent> productComponentsList;
        private TextView productTextView, instructionsTextView, noComponentsTextView;
        private Button previousButton;
        private SwapProduct _product;
        public ProductComponentsResponse _productComponentsResponse;
        private CustomerDetailsResponse _customerDetailsResponse;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Bundle extras = Intent.Extras;
            _product = JsonConvert.DeserializeObject<SwapProduct>(extras.GetString("product"));
            _productComponentsResponse = JsonConvert.DeserializeObject<ProductComponentsResponse>(extras.GetString("productComponentsResponse"));
            _customerDetailsResponse = JsonConvert.DeserializeObject<CustomerDetailsResponse>(extras.GetString("customerDetailsResponse"));
            InitializeScreen();
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.layout_product_components);
            SetScreenTitle(GetString(Resource.String.component_swap));
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            //init
            productComponentsList = new List<ProductComponent>();

            productTextView = FindViewById<TextView>(Resource.Id.textView_product_name);
            instructionsTextView = FindViewById<TextView>(Resource.Id.textView_instructions);
            noComponentsTextView = FindViewById<TextView>(Resource.Id.textView_no_components);
            previousButton = FindViewById<Button>(Resource.Id.button_previous);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView_products);

            UpdateScreen();

            layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.AddItemDecoration(new SpaceItemDecoration(this, Resource.Dimension.recycler_items_space, true, true));
            adapter = new ProductComponentsAdapter(_productComponentsResponse, this, _product, _customerDetailsResponse);
            recyclerView.SetAdapter(adapter);

            SetListeners();
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            //            if (SwapComponentsActivity.productComponentsResponse != null)
            //            {
            //                productComponentsResponse = SwapComponentsActivity.productComponentsResponse;
            if (_productComponentsResponse.ProductComponents != null)
            {
                productComponentsList = _productComponentsResponse.ProductComponents;
                if (productComponentsList.Count == 0)
                {
                    showNoComponentsMessage();
                }
            }
            else
            {
                productComponentsList = new List<ProductComponent>();
                showNoComponentsMessage();
            }
            productTextView.Text = GetString(Resource.String.product_)+" " + _product.ProductName;
            //            }
            //            else
            //            {
            //                showNoComponentsMessage();
            //            }

        }

        private void showNoComponentsMessage(string customMessage = null)
        {
            productTextView.Visibility = ViewStates.Gone;
            instructionsTextView.Visibility = ViewStates.Gone;
            recyclerView.Visibility = ViewStates.Gone;
            string message = GetString(Resource.String.no_components);
            if (customMessage != null)
            {
                message = customMessage;
            }
            noComponentsTextView.Visibility = ViewStates.Visible;
            noComponentsTextView.Text = message;
            previousButton.Text = GetString(Resource.String.select_another_product);
        }

        public override void SetListeners()
        {
            previousButton.Click += delegate
            {
                OnBackPressed();
            };

            //recyclerView.AddOnScrollListener(new MScrollListener(this));
        }

        public override bool Validate()
        {
            return true;
        }

        public override void OnBackPressed()
        {
            Intent intent = new Intent(this, typeof(CustomerDetailsActivity));
            Bundle extras = new Bundle();
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
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

        //public class MScrollListener : RecyclerView.OnScrollListener
        //{
        //    private ProductComponentsActivity productComponentsActivity;

        //    public MScrollListener(ProductComponentsActivity productComponentsActivity)
        //    {
        //        this.productComponentsActivity = productComponentsActivity;
        //    }
        //    public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        //    {
        //        base.OnScrolled(recyclerView, dx, dy);
        //        var visibleItemCount = recyclerView.ChildCount;
        //        var totalItemCount = productComponentsActivity.adapter.ItemCount;

        //        LinearLayoutManager layoutManager = ((LinearLayoutManager)recyclerView.GetLayoutManager());
        //        int lastVisibleItem = layoutManager.FindLastCompletelyVisibleItemPosition();
        //        Log.Verbose("OnScrolled visibleItemCount " + visibleItemCount + " totalItemCount " + totalItemCount + " lastVisibleItem" + lastVisibleItem);
        //    }
        //}
    }
}