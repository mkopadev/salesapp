using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using LogManager = SalesApp.Core.Logging.LogManager;

namespace SalesApp.Droid.People.SwapComponents.Models
{
    class ProductComponentsAdapter : RecyclerView.Adapter
    {
        private static readonly ILog Log = LogManager.Get(typeof(ProductComponentsAdapter));
        private List<ProductComponent> _productComponentsList;
        private ProductComponentsResponse _productComponentsResponse;
        private static Context context;
        private SwapProduct _product;
        private SwapComponentApi _api;
        private CustomerDetailsResponse _customerDetailsResponse;

        // Initialize the dataset of the Adapter
        public ProductComponentsAdapter(ProductComponentsResponse productComponentsResponse, Context context, SwapProduct product, CustomerDetailsResponse customerDetailsResponse)
        {
            this._productComponentsResponse = productComponentsResponse;
            this._productComponentsList = productComponentsResponse.ProductComponents;
            ProductComponentsAdapter.context = context;
            this._product = product;
            this._customerDetailsResponse = customerDetailsResponse;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            View v = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.product_component_item, viewGroup, false);
            ViewHolder vh = new ViewHolder(v);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            // Get element from your dataset at this position and replace the contents of the view
            // with that element
            ProductComponent productComponent = _productComponentsList[position];
            string name = productComponent.StockItem;
            string componentName = name.ToLower();
            (viewHolder as ViewHolder).NameTextView.SetText(name, TextView.BufferType.Normal);
            Log.Verbose(DateTimeExtensions.GetDateStandardFormat(_product.AllocationDate));
            DateTime warrantyExpiryDate = _product.AllocationDate.AddDays(productComponent.WarrantyPeriod);
            String warrantyMessage;
            if (CustomerDetailsActivity.WarrantyIsExpired(warrantyExpiryDate))
            {
                warrantyMessage = context.GetString(Resource.String.warranty_expired_on) + " ";
                //                (viewHolder as ViewHolder).TextViewWarranlor.whittyExpiry
                //                    .SetBackgroundResource(Resource.Drawable.border_rounded_warranty_red);
                //                (viewHolder as ViewHolder).TextViewWarrantyExpiry.SetTextColor(
                //                    context.Resources.GetColor(Resource.Coe));
            }
            else
            {
                warrantyMessage = context.GetString(Resource.String.in_warranty_until) + " ";
                //                (viewHolder as ViewHolder).TextViewWarrantyExpiry
                //                   .SetBackgroundResource(Resource.Drawable.border_rounded_warranty);
                //                (viewHolder as ViewHolder).TextViewWarrantyExpiry.SetTextColor(
                //                    context.Resources.GetColor(Resource.Color.grey_dark));
            }
            (viewHolder as ViewHolder).WarrantyTextView.SetText(warrantyMessage, TextView.BufferType.Normal);
            (viewHolder as ViewHolder).WarrantyDateTextView.SetText(DateTimeExtensions.GetDateStandardFormat(warrantyExpiryDate), TextView.BufferType.Normal);

            int image = Resource.Drawable.salesapp_logo;

            if (componentName.Contains("bulb"))
            {
                image = Resource.Drawable.ic_bulb;
            }
            else if (componentName.Contains("usb") || componentName.Contains("charger"))
            {
                image = Resource.Drawable.ic_cables;
            }
            else if (componentName.Contains("panel"))
            {
                image = Resource.Drawable.ic_panel;
            }
            else if (componentName.Contains("control") || componentName.Contains("ibis"))
            {
                image = Resource.Drawable.ic_ibis;
            }
            else if (componentName.Contains("radio") || componentName.Contains("fm"))
            {
                image = Resource.Drawable.ic_radio;
            }
            else if (componentName.Contains("flash") || componentName.Contains("light") || componentName.Contains("torch"))
            {
                image = Resource.Drawable.ic_flashlight;
            }
            else if (componentName.Contains("cable") || componentName.Contains("connector"))
            {
                image = Resource.Drawable.ic_connector_cable;
            }
            if (image != 0)
            {
                try
                {
                    (viewHolder as ViewHolder).ComponentImageView.SetImageResource(image);
                }
                catch (Exception e)
                {
                    //set a default image
                    Log.Verbose("No image");
                }
            }
            (viewHolder as ViewHolder).RootLayout.Click += delegate
            {
                if (!CustomerDetailsActivity.WarrantyIsExpired(_product.AllocationDate.AddDays(productComponent.WarrantyPeriod)))
                {
                    bool isOnline = Resolver.Instance.Get<IConnectivityService>().HasConnection();
                    if (!isOnline)
                    {
                        Toast.MakeText(context, context.GetString(Resource.String.unable_to_continue_net),
                            ToastLength.Long).Show();
                    }
                    else
                    {
                        GotoSwapProductComponent(productComponent);
                    }
                }
                else
                {
                    //                    Toast.MakeText(context, "The warranty for this component is expired. Select another component.",
                    //                            ToastLength.Long).Show();
                    Intent intent = new Intent(context, typeof(WarrantyExpiredActivity));
                    //pass the product details
                    Bundle extras = new Bundle();
                    extras.PutString("product", JsonConvert.SerializeObject(_product));
                    extras.PutString("productComponent", JsonConvert.SerializeObject(productComponent));
                    extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                    extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                    intent.PutExtras(extras);
                    context.StartActivity(intent);
                }
            };
        }

        private async void GotoSwapProductComponent(ProductComponent productComponent)
        {
            Log.Verbose("Get compatible components of a product.");
            var progressDialog = ProgressDialog.Show(context, context.GetString(Resource.String.please_wait), context.GetString(Resource.String.loading_compatible_components), true);
            _api = new SwapComponentApi();
            string parameters = "/compatible?stockId=" + productComponent.StockId;
            ProductComponentsResponse swappableProductComponentsResponse = await _api.GetProductDetails(Uri.EscapeUriString(parameters),
                filterFlags: ErrorFilterFlags.AllowEmptyResponses);
            progressDialog.Hide();
            if (swappableProductComponentsResponse.Successful)
            {
                Log.Verbose("API Call successful");
                Intent intent = new Intent(context, typeof(SwappableComponentsActivity));
                //pass the product details
                Bundle extras = new Bundle();
                extras.PutString("product", JsonConvert.SerializeObject(_product));
                extras.PutString("productComponent", JsonConvert.SerializeObject(productComponent));
                extras.PutString("swappableProductComponentsResponse", JsonConvert.SerializeObject(swappableProductComponentsResponse));
                extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
                extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
                intent.PutExtras(extras);
                context.StartActivity(intent);
            }
            else
            {
                Log.Verbose("Something went wrong");
                if (swappableProductComponentsResponse.ResponseText.Equals("not_connected"))
                {
                    Toast.MakeText(context, context.GetString(Resource.String.not_connected), ToastLength.Long).Show();
                }
            }

        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get { return _productComponentsList.Count(); }
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class ViewHolder : RecyclerView.ViewHolder
        {
            private TextView nameTextView, warrantyTextView, warrantyDateTextView;
            private ImageView componentImageView;
            private RelativeLayout rootLayout;

            public TextView NameTextView
            {
                get { return nameTextView; }
            }
            public TextView WarrantyTextView
            {
                get { return warrantyTextView; }
            }
            public TextView WarrantyDateTextView
            {
                get { return warrantyDateTextView; }
            }
            public ImageView ComponentImageView
            {
                get { return componentImageView; }
            }
            public RelativeLayout RootLayout
            {
                get { return rootLayout; }
            }
            public ViewHolder(View v)
                : base(v)
            {
                nameTextView = (TextView)v.FindViewById(Resource.Id.textView_component_name);
                warrantyTextView = (TextView)v.FindViewById(Resource.Id.textView_warranty);
                warrantyDateTextView = (TextView)v.FindViewById(Resource.Id.textView_warranty_date);
                componentImageView = (ImageView)v.FindViewById(Resource.Id.imageView_component);
                rootLayout = (RelativeLayout)v.FindViewById(Resource.Id.root_layout);
            }
        }

    }
}