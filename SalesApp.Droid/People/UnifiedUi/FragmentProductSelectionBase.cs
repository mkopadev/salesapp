using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi
{
    /// <summary>
    /// This class is the base class for loading products as buttons
    /// </summary>
    public abstract class FragmentProductSelectionBase : WizardStepFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        /// <summary>
        /// Gets the step title
        /// </summary>
        public override int StepTitle
        {
            get { return Resource.String.unified_select_product_title; }
        }

        /// <summary>
        /// Gets a list of products
        /// </summary>
        protected List<Product> Products
        {
            get
            {
                string productsJson = Settings.Instance.Products;
                return JsonConvert.DeserializeObject<List<Product>>(productsJson);
            }
        }

        /// <summary>
        /// Gets the lead that we are selecting the product for
        /// </summary>
        protected abstract Lead Lead { get; }

        /// <summary>
        /// Called when the product button is clicked
        /// </summary>
        /// <param name="sender">The button that was clicked</param>
        /// <param name="e">The event args</param>
        public virtual void ProductButtonClick(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string displayName = clickedButton.Text;
            this.ProductSelected(displayName);
        }

        /// <summary>
        /// Creates the view for this fragment
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container</param>
        /// <param name="savedInstanceState">The saved state</param>
        /// <returns>The view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.FragmentView = inflater.Inflate(Resource.Layout.fragment_layout_select_product_buttons, container, false);

            this.WizardActivity.ButtonNextEnabled = false;
            this.WizardActivity.ButtonNext.Visibility = ViewStates.Gone;

            // load the buttons
            this.LoadProductButtons();

            return this.FragmentView;
        }

        /// <summary>
        /// Called when a serial number is selected
        /// </summary>
        /// <param name="displayName">The text</param>
        protected void ProductSelected(string displayName)
        {
            this.Activity.RunOnUiThread(
                    () =>
                    {
                        var product = this.Products.SingleOrDefault(prod => prod.DisplayName.AreEqual(displayName));

                        if (product == null)
                        {
                            return;
                        }

                        this.Lead.Product = new Product
                        {
                            ProductTypeId = product.ProductTypeId,
                            DisplayName = product.DisplayName
                        };
                    });
        }

        /// <summary>
        /// This inflates the products buttons using the OTA settings into the given container
        /// </summary>
        private void LoadProductButtons()
        {
            LayoutInflater inflater = LayoutInflater.From(this.Activity);
            LinearLayout productsContainer = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.productsContainer);
            if (productsContainer == null)
            {
                throw new NullPointerException("The container for the product buttons cannot be null!");
            }

            if (productsContainer.ChildCount > 0)
            {
                // Buttons already inflated
                return;
            }

            int index = 0;

            foreach (var product in this.Products)
            {
                LinearLayout layout = (LinearLayout)inflater.Inflate(Resource.Layout.product_button, productsContainer);

                Button productButton = (Button)layout.GetChildAt(index);

                if (productButton == null)
                {
                    // A child of the buttons container is not a button. Stop!
                    throw new IllegalArgumentException("All the children of the product buttons Linear Layout must be buttons");
                }

                productButton.Text = product.DisplayName;
                productButton.Click += this.ProductButtonClick;

                index++;
            }
        }
    }
}