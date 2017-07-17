using System;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    /// <summary>
    /// This is the product selection screen that uses buttons instead of spinners
    /// </summary>
    public class FragmentCustomerProductSelection : FragmentProductSelectionBase
    {
        /// <summary>
        /// The registration information
        /// </summary>
        private SalesApp.Core.BL.Models.People.Customer personRegistrationInfo;

        /// <summary>
        /// Gets the customer that we are selecting the product for
        /// </summary>
        protected override Lead Lead
        {
            get
            {
                return this.personRegistrationInfo;
            }
        }

        /// <summary>
        /// Load the registration data from a JSON string
        /// </summary>
        /// <param name="serializedString">The JSON string</param>
        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                this.personRegistrationInfo =
                    JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);
                if (this.personRegistrationInfo.Product == null)
                {
                    this.personRegistrationInfo.Product = new Product();
                }
            }
        }

        /// <summary>
        /// Get the registration data
        /// </summary>
        /// <returns>The string representation of the data</returns>
        public override string GetData()
        {
            return JsonConvert.SerializeObject(this.personRegistrationInfo);
        }

        /// <summary>
        /// Get the next fragment
        /// </summary>
        /// <returns>The next fragment to load</returns>
        public override Type GetNextFragment()
        {
            //            return typeof(FragmentGroupSelectionYesNo);
            var photoFeatureEnabled = Settings.Instance.PhotoFeatureEnabled;
            if (photoFeatureEnabled == 1)
            {
                return typeof(CustomerPhotoFragment);
            }
            return typeof(FragmentCustomerConfirmationScreen);
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
            if (savedInstanceState != null)
            {
                string regInfo = savedInstanceState.GetString(BundledRegistrationInfo);
                if (!regInfo.IsBlank())
                {
                    this.personRegistrationInfo =
                        JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(regInfo);
                }
            }

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Product Selection");

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                string regInfo = savedInstanceState.GetString(BundledRegistrationInfo);
                if (!regInfo.IsBlank())
                {
                    this.personRegistrationInfo =
                        JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(regInfo);
                }

            }
        }
        /// <summary>
        /// Save our current state into a bundle
        /// </summary>
        /// <param name="outState">The bundle</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(BundledRegistrationInfo, JsonConvert.SerializeObject(this.personRegistrationInfo));
        }

        /// <summary>
        /// Validate fragment data
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        public override bool Validate()
        {
            if (this.personRegistrationInfo.Product == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// What happens before we go next
        /// </summary>
        /// <returns>The validation before going to the next fragment</returns>
        public override bool BeforeGoNext()
        {
            return true;
        }

        /// <summary>
        /// Called when the product button is clicked
        /// </summary>
        /// <param name="sender">The button that was clicked</param>
        /// <param name="e">The event args</param>
        public override void ProductButtonClick(object sender, EventArgs e)
        {
            base.ProductButtonClick(sender, e);
            this.WizardActivity.GoNext();
        }
    }
}