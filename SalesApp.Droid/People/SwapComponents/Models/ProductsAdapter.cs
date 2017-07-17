using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Logging;
using Enumerable = System.Linq.Enumerable;

namespace SalesApp.Droid.People.SwapComponents.Models
{
    class ProductsAdapter : RecyclerView.Adapter
    {
        private static readonly ILog Log = LogManager.Get(typeof(ProductsAdapter));
        public const string TAG = "ProductsAdapter";
        private List<SwapProduct> productsList;
        private static Context context;
        public event EventHandler<int> ItemClick;

        // Initialize the dataset of the Adapter
        public ProductsAdapter(List<SwapProduct> productsList, Context context)
        {
            this.productsList = productsList;
            ProductsAdapter.context = context;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int position)
        {
            View itemView = LayoutInflater.From(viewGroup.Context)
                .Inflate(Resource.Layout.product_item, viewGroup, false);
            ViewHolder vh = new ViewHolder(itemView, OnClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            // Get element from your dataset at this position and replace the contents of the view
            // with that element
            SwapProduct product = productsList[position];
            (viewHolder as ViewHolder).ProductButton.SetText(product.ProductName, TextView.BufferType.Normal);
//            try
//            {
//                String warrantyMessage;
//                if (CustomerDetailsActivity.WarrantyIsExpired(product.WarrantyExpiryDate))
//                {
//                    warrantyMessage = "Warranty expired ";
//                    (viewHolder as ViewHolder).TextViewWarrantyExpiry
//                        .SetBackgroundResource(Resource.Drawable.border_rounded_warranty_red);
//                    //(viewHolder as ViewHolder).TextViewWarrantyExpiry.SetBackgroundColor(context.Resources.GetColor(Resource.Color.red));
//                    (viewHolder as ViewHolder).TextViewWarrantyExpiry.SetTextColor(
//                        context.Resources.GetColor(Resource.Color.white));
//                }
//                else
//                {
//                    warrantyMessage = "Warranty until ";
//                    (viewHolder as ViewHolder).TextViewWarrantyExpiry
//                       .SetBackgroundResource(Resource.Drawable.border_rounded_warranty);
//                    (viewHolder as ViewHolder).TextViewWarrantyExpiry.SetTextColor(
//                        context.Resources.GetColor(Resource.Color.grey_dark));
//                }
//                (viewHolder as ViewHolder).TextViewWarrantyExpiry.SetText(warrantyMessage + DateTimeExtensions.GetDateStandardFormat(product.WarrantyExpiryDate), TextView.BufferType.Normal);
//            }
//            catch (Exception e)
//            {
//                Log.Verbose("Exception " + e.Message);
//            }
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get { return Enumerable.Count(productsList); }
        }

        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class ViewHolder : RecyclerView.ViewHolder
        {
            private Button productButton;
            //            private TextView textViewWarrantyExpiry;

            public Button ProductButton
            {
                get { return productButton; }
            }

            //            public TextView TextViewWarrantyExpiry
            //            {
            //                get { return textViewWarrantyExpiry; }
            //            }

            public ViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                productButton = (Button)itemView.FindViewById(Resource.Id.button_product_name);
                //                textViewWarrantyExpiry = (TextView)itemView.FindViewById(Resource.Id.textView_warranty_expiry);
                productButton.Click += (sender, e) => listener(base.Position);

            }
        }

    }
}