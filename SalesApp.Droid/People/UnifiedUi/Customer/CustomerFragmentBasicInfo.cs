using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Utils.ViewsHelper;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    public class CustomerFragmentBasicInfo : FragmentBasicInfo
    {
        public SalesApp.Core.BL.Models.People.Customer Customer { get; set; }

        private ViewsHelper<SalesApp.Core.BL.Models.People.Customer> viewsHelper;

        private bool _madeCheck = false;

        public override void WriteViews()
        {
            viewsHelper.WriteBoundViews(Customer);
        }

        public override void OnResume()
        {
            base.OnResume();
            viewsHelper.ViewEnabledPredicates(
                WizardActivity.ButtonNext,
                new List<KeyValuePair<int, Predicate<string>>>
                {
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtLastName, ValidateHasContent),
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtFirstName, ValidateHasContent),
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtId, ValidateHasContent),
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtPhone, ValidateHasContent),
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtPhone, PhoneNumberLooksOk)
                });

            viewsHelper.SetViewEnabledState(WizardActivity.ButtonNext);
        }

        public override void CreateView()
        {

            viewsHelper = new ViewsHelper<SalesApp.Core.BL.Models.People.Customer>(this.Activity as ActivityBase, this.View);
            if (Customer == null)
            {
                Customer = new SalesApp.Core.BL.Models.People.Customer();

                Customer.Product = new Product();
            }

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Customer basic info");
        }

        public override void SetData()
        {
            SetData(JsonConvert.SerializeObject(Customer));
        }

        public override void SetData(string serializedString)
        {
            if (serializedString.IsBlank() == false)
            {
                Customer = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);

                if (viewsHelper != null)
                {
                    viewsHelper.WriteBoundViews(Customer);
                }
            }
        }

        public override string GetData()
        {
            viewsHelper.Read();
            if (Customer == null)
            {
                return "";
            }

            Customer = viewsHelper.Read();
            return JsonConvert.SerializeObject(Customer);
        }

        public override void FillFromExistingRecord(Person person)
        {
            if (person != null)
            {
                viewsHelper.Write(Resource.Id.edtPhone, person.Phone)
                .Write(Resource.Id.edtLastName, person.LastName)
                .Write(Resource.Id.edtId, string.IsNullOrWhiteSpace(person.NationalId) ? Customer.NationalId : person.NationalId)
                .Write(Resource.Id.edtFirstName, person.FirstName);
            }
        }

        public override void BindViews()
        {
            viewsHelper
                .BindEditText(Resource.Id.edtPhone, "Phone")
                .BindEditText(Resource.Id.edtLastName, "LastName")
                .BindEditText(Resource.Id.edtFirstName, "FirstName")
                .BindEditText(Resource.Id.edtId, "NationalId")
                .BindEvent(BindableEvents.OnLostFocus, Resource.Id.edtPhone, ValidatePhoneNumber);
            viewsHelper.CompileBindings();
            SetData();
        }

        public override Type GetNextFragment()
        {
            return typeof(FragmentCustomerProductSelection);
        }

        public override bool Validate()
        {
            Customer = viewsHelper.Read(true);

            bool phoneIsDuplicate = false;
            if (EnableDuplicateChecking)
            {
                phoneIsDuplicate = AsyncHelper.RunSync(async () => await ShowOverlayIfPhoneNumberDuplicate(Customer.Phone, Customer.NationalId));
                _madeCheck = phoneIsDuplicate;
            }

            if (IsAdditionalProduct)
            {
                Customer.IsAdditionalProduct = true;
            }

            if (phoneIsDuplicate)
            {
                return false;
            }

            bool valid = Validator.ValidateAll(View);

            if (!valid)
            {
                return false;
            }

            if (!_madeCheck && !Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                //since we don't have internet,
                //display a dialog to choose whether this is an additional product since we cannot(have tried locally already)
                return AsyncHelper.RunSync(async () => await ShowOverlayIfOffline(Customer));
            }

            return true;
        }
    }
}