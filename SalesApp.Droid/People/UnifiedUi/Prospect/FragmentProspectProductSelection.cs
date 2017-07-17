using System;
using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    /// <summary>
    /// This is the product selection screen that uses buttons instead of spinners
    /// </summary>
    public class FragmentProspectProductSelection : FragmentProductSelectionBase
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // App tracking
            GoogleAnalyticService.Instance.TrackScreen("Prospect Product Selection");
            RetainInstance = true;
        }

        /// <summary>
        /// The registration information
        /// </summary>
        private SalesApp.Core.BL.Models.People.Prospect personRegistrationInfo;

        /// <summary>
        /// Gets the prospect that we are selecting the product for
        /// </summary>
        protected override Lead Lead
        {
            get
            {
                return this.personRegistrationInfo;
            }
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

        /// <summary>
        /// Load the registration data from a JSON string
        /// </summary>
        /// <param name="serializedString">The JSON string</param>
        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                this.personRegistrationInfo = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(serializedString);

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
            return typeof(FragmentScoreProspect);
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

        /*/// <summary>
        /// Initialize our UI
        /// </summary>
        /// <param name="isOnUiThread">Whether or not we are on the UI thread</param>
        protected override void InitializeUI(bool isOnUiThread = false)
        {
            base.InitializeUI(isOnUiThread);
            if (!isOnUiThread)
            {
                this.Activity.RunOnUiThread(
                        () =>
                        {
                            this.InitializeUI(true);
                        });

                return;
            }

            if (this.personRegistrationInfo == null)
            {
                throw new Exception("No data was passed to this fragment, customer registration information should never be null here");
            }

            this.WizardActivity.ButtonNextEnabled = false;
        }*/
    }
}