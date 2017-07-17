using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Utils.ViewsHelper;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    public class ProspectFragmentBasicInfo : FragmentBasicInfo
    {
        public SalesApp.Core.BL.Models.People.Prospect Prospect { get; set; }

        private ViewsHelper<SalesApp.Core.BL.Models.People.Prospect> viewsHelper;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // App tracking
            GoogleAnalyticService.Instance.TrackScreen("Prospect basic information");

            RetainInstance = true;
        }

        public override void SetData(string serializedString)
        {
            if (serializedString.IsBlank() == false)
            {
                Prospect = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(serializedString);

                if (viewsHelper != null)
                {
                    viewsHelper.WriteBoundViews(Prospect);
                }
            }
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
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtPhone, ValidateHasContent),
                    new KeyValuePair<int, Predicate<string>>(Resource.Id.edtPhone, PhoneNumberLooksOk)
                });

            viewsHelper.SetViewEnabledState(WizardActivity.ButtonNext);
        }

        public override string GetData()
        {
            Prospect = viewsHelper.Read();
            if (Prospect == null)
            {
                return string.Empty;
            }

            // Prospect = viewsHelper.Read();
            return JsonConvert.SerializeObject(Prospect);
        }

        public override Type GetNextFragment()
        {
            return typeof(FragmentProspectProductSelection);
        }

        public override bool Validate()
        {
            this.Prospect = this.viewsHelper.Read(true);

            bool phoneIsDuplicate = false;
            if (this.EnableDuplicateChecking)
            {
                phoneIsDuplicate = Task.Run(async() => await ShowOverlayIfPhoneNumberDuplicate(Prospect.Phone)).Result;
            }

            if (phoneIsDuplicate)
            {
                return false;
            }
            bool validationResult = false;
            validationResult = Validator.ValidateAll(View, true);

            return validationResult;
        }

        public override void FillFromExistingRecord(Person person)
        {
            if (person != null)
            {
                viewsHelper.Write(Resource.Id.edtPhone, person.Phone)
                .Write(Resource.Id.edtLastName, person.LastName)
                .Write(Resource.Id.edtFirstName, person.FirstName);
            }
        }

        public override void BindViews()
        {
            viewsHelper
                .BindEditText(Resource.Id.edtPhone, "Phone")
                .BindEditText(Resource.Id.edtLastName, "LastName")
                .BindEditText(Resource.Id.edtFirstName, "FirstName")
                .BindEvent(BindableEvents.OnLostFocus, Resource.Id.edtPhone, ValidatePhoneNumber);

            viewsHelper.CompileBindings();
            SetData();
        }

        public override void SetData()
        {
            SetData(JsonConvert.SerializeObject(Prospect));
        }

        public override void WriteViews()
        {
            viewsHelper.WriteBoundViews(Prospect);
        }

        public override void CreateView()
        {
            viewsHelper = new ViewsHelper<SalesApp.Core.BL.Models.People.Prospect>(Activity as ActivityBase, View);
            if (Prospect == null)
            {
                Prospect = new SalesApp.Core.BL.Models.People.Prospect();
            }
        }
    }
}