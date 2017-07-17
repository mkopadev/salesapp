using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    /// <summary>
    /// This is the prospect info confirmation screen
    /// </summary>
    public class FragmentProspectConfirmationScreen : FragmentConfirmationScreen
    {
        /// <summary>
        /// The registration information
        /// </summary>
        private SalesApp.Core.BL.Models.People.Prospect PersonRegistrationInfo;

        /// <summary>
        /// Gets the step title
        /// </summary>
        public override int StepTitle
        {
            get { return Resource.String.unified_prospect_information_title; }
        }

        /// <summary>
        /// Gets the prospect that we are selecting the product for
        /// </summary>
        protected override Lead Lead
        {
            get
            {
                return this.PersonRegistrationInfo;
            }
        }

        protected override List<GroupKeyValue> ConfirmationDetials
        {
            get
            {
                string nameLabel = this.GetString(Resource.String.unified_name);
                string phoneLabel = this.GetString(Resource.String.unified_phone);
                string productLabel = this.GetString(Resource.String.unified_product);
                string scoreLabel = this.GetString(Resource.String.unified_score);

                List<GroupKeyValue> confirmationDetails = new List<GroupKeyValue>
                {
                    new GroupKeyValue { Key = nameLabel, Name = this.Lead.FullName },
                    new GroupKeyValue { Key = phoneLabel, Name = this.Lead.Phone },
                    new GroupKeyValue { Key = productLabel, Name = this.Lead.Product.DisplayName },
                    new GroupKeyValue { Key = scoreLabel, Name = this.GetScore(this.PersonRegistrationInfo) }
                };

                if (this.Lead.GroupInfo == null)
                {
                    return confirmationDetails;
                }

                foreach (var groupInfo in this.Lead.GroupInfo)
                {
                    confirmationDetails.Add(groupInfo);
                }

                return confirmationDetails;
            }
        }

        /// <summary>
        /// What happens before we go next
        /// </summary>
        /// <returns>The validation before going to the next fragment</returns>
        public override bool BeforeGoNext()
        {
            bool savedProspect = AsyncHelper.RunSync(
                    async () =>
                    {
                        ProspectsController prospectsController = new ProspectsController();
                        this.PersonRegistrationInfo.Groups =
                            JsonConvert.SerializeObject(this.PersonRegistrationInfo.GroupInfo);
                        await prospectsController.SaveAsync(this.PersonRegistrationInfo);
                        return this.PersonRegistrationInfo.Id != default(Guid);
                    });

            return savedProspect;
        }

        /// <summary>
        /// Load the registration data from a JSON string
        /// </summary>
        /// <param name="serializedString">The JSON string</param>
        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                this.PersonRegistrationInfo =
                    JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(serializedString);
                if (this.PersonRegistrationInfo.Product == null)
                {
                    this.PersonRegistrationInfo.Product = new Product();
                }
            }
        }

        /// <summary>
        /// Get the registration data
        /// </summary>
        /// <returns>The string representation of the data</returns>
        public override string GetData()
        {
            return JsonConvert.SerializeObject(this.PersonRegistrationInfo);
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
            base.OnCreateView(inflater, container, savedInstanceState);

            //this.FragmentView.FindViewById<TextView>(Resource.Id.tvScore).Text = this.GetScore(this.PersonRegistrationInfo);
            //this.RowId.Visibility = ViewStates.Gone;

            return this.FragmentView;
        }

        /// <summary>
        /// Get the next fragment to load
        /// </summary>
        /// <returns>The type of the next fragment to be loaded</returns>
        public override Type GetNextFragment()
        {
            return typeof(FragmentFollowupReminder);
        }

        /// <summary>
        /// Gets the score as a string
        /// </summary>
        /// <param name="prospect">The prospect</param>
        /// <returns>The the prospect's score as a string</returns>
        private string GetScore(SalesApp.Core.BL.Models.People.Prospect prospect)
        {
            var score = prospect.Score;

            switch (score)
            {
                case 3:
                    return this.Activity.GetString(Resource.String.unified_hot);
                case 2:
                    return this.Activity.GetString(Resource.String.unified_warm);
                default:
                    return this.Activity.GetString(Resource.String.unified_cold);
            }
        }

        
    }
}