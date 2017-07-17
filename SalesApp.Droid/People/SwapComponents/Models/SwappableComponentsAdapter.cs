using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Logging;

namespace SalesApp.Droid.People.SwapComponents.Models
{
    class SwappableComponentsAdapter : RecyclerView.Adapter
    {
        private static readonly ILog Log = LogManager.Get(typeof(ProductComponentsAdapter));
        private List<ProductComponent> _swappableProductComponentsList;
        private static Context context;
        private ProductComponent _selectedProductComponent;
        private SwapProduct _product;
        private CustomerDetailsResponse _customerDetailsResponse;
        private ProductComponentsResponse _swappableProductComponentsResponse, _productComponentsResponse;

        // Initialize the dataset of the Adapter
        public SwappableComponentsAdapter(ProductComponentsResponse swappableProductComponentsResponse, Context context, ProductComponent selectedProductComponent,
            CustomerDetailsResponse customerDetailsResponse, SwapProduct product, ProductComponentsResponse productComponentsResponse)
        {
            _swappableProductComponentsResponse = swappableProductComponentsResponse;
            _swappableProductComponentsList = swappableProductComponentsResponse.ProductComponents;
            _selectedProductComponent = selectedProductComponent;
            _customerDetailsResponse = customerDetailsResponse;
            _product = product;
            _productComponentsResponse = productComponentsResponse;
            SwappableComponentsAdapter.context = context;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            View v = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.swappable_component_item, viewGroup, false);
            ViewHolder vh = new ViewHolder(v);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            // Get element from your dataset at this position and replace the contents of the view
            // with that element
            ProductComponent productComponent = _swappableProductComponentsList[position];
            string name = productComponent.Product + " " + productComponent.StockItem;
            string componentName = name.ToLower();
            (viewHolder as ViewHolder).NameTextView.SetText(name, TextView.BufferType.Normal);
            (viewHolder as ViewHolder).InStockTextView.SetText(productComponent.InStock + "", TextView.BufferType.Normal);
            (viewHolder as ViewHolder).ReturnedTextView.SetText(productComponent.Received + "", TextView.BufferType.Normal);
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
                if (productComponent.InStock > 0)
                {
                    GotoSwapProductComponent(productComponent);
                }
                else
                {
                    Toast.MakeText(context, context.GetString(Resource.String.out_of_stock_component), ToastLength.Long).Show();
                }
            };
        }

        private void GotoSwapProductComponent(ProductComponent productComponent)
        {
            Intent intent = new Intent(context, typeof(ReasonForSwapActivity));
            //pass the product details
            Bundle extras = new Bundle();
            extras.PutString("incomingProductComponent", JsonConvert.SerializeObject(_selectedProductComponent));
            extras.PutString("outgoingProductComponent", JsonConvert.SerializeObject(productComponent));
            extras.PutString("product", JsonConvert.SerializeObject(_product));
            extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(_customerDetailsResponse));
            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(_productComponentsResponse));
            extras.PutString("productComponent", JsonConvert.SerializeObject(_selectedProductComponent));
            extras.PutString("swappableProductComponentsResponse", JsonConvert.SerializeObject(_swappableProductComponentsResponse));
            //            extras.PutString("productComponentsResponse", JsonConvert.SerializeObject(productComponentsResponse));
            //            extras.PutInt("incomingStockId", _selectedProductComponent.StockId);
            //            extras.PutInt("outgoingStockId", productComponent.StockId);
            //            extras.PutString("incomingComponentName", productComponent.StockItem);
            //            extras.PutInt("incomingProductId", productComponent.ProductId);
            //            extras.PutString("incomingProductName", productComponent.Product);
            intent.PutExtras(extras);
            //start activity
            context.StartActivity(intent);
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get { return _swappableProductComponentsList.Count(); }
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class ViewHolder : RecyclerView.ViewHolder
        {
            private TextView nameTextView, inStockTextView, returnedTextView;
            private ImageView componentImageView;
            private RelativeLayout rootLayout;

            public TextView NameTextView
            {
                get { return nameTextView; }
            }
            public TextView InStockTextView
            {
                get { return inStockTextView; }
            }
            public TextView ReturnedTextView
            {
                get { return returnedTextView; }
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
                inStockTextView = (TextView)v.FindViewById(Resource.Id.textView_in_stock_number);
                returnedTextView = (TextView)v.FindViewById(Resource.Id.textView_returned_number);
                componentImageView = (ImageView)v.FindViewById(Resource.Id.imageView_component);
                rootLayout = (RelativeLayout)v.FindViewById(Resource.Id.root_layout);

            }
        }

    }
}