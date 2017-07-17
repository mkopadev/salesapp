using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mkopa.Core.BL.Controllers.People;
using Mkopa.Core.BL.Models.People;
using Mkopa.Core.Extensions;
using MK.Solar.Api;
using MK.Solar.Components.UIComponents;
using Newtonsoft.Json;

//using MK.Solar.BL;
//using MK.Solar.BL.Managers;

namespace MK.Solar.People.Prospects
{
    public class BasicInfoFragment : FragmentBase , INavigableFragment
    {
        private BasicInfoView basicInfo;
        private IRegistrationCoordinator coordinator;
        private Activity _activity;
        public int NextButtonText { get { return Resource.String.next; } }
        public int CancelButtonText { get { return Resource.String.cancel; } }
        private ProspectsController _prospController;
        private const string BundledProspect = "BundledProspect";
        public const string FragmentTagBasicInfoFragment = "FragmentTagBasicInfoFragment";

        public ProspectsController ProspController
        {
            get
            {
                if (_prospController == null)
                {
                    Settings settings = new Settings(_activity);
                    _prospController = new ProspectsController(SolarApplication.SolarDatabase, settings.DsrLanguage, settings.DsrCountryCode);
                }
                return _prospController;
            }
        }
        protected override void UpdateUI()
        {

        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            _activity = activity;
            coordinator = (IRegistrationCoordinator) activity;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (coordinator != null)
            {
                string bundledProspect = savedInstanceState.GetString(BundledProspect);
                if (bundledProspect.IsBlank() == false)
                {
                    coordinator.Prospect = JsonConvert.DeserializeObject<Prospect>(bundledProspect);
                }
            }
        }

        protected override void InitializeUI()
        {
            
        }

        protected override void SetEventHandlers()
        {
            
        }

        

        public bool EnableNextButton
        {
            get { return basicInfo.IsAllValid(); }
        }

        public override void OnResume ()
		{
			base.OnResume ();
			basicInfo.CheckPerson += HandleCheckPerson;
			basicInfo.ValidProspect += ValidProspect;
			basicInfo.Cancel += Cancel;
            if (coordinator.Prospect != null)
            {
                coordinator.onProgress(this);
            }
		}

        public async Task NextClicked()
        {
            coordinator.ShowWait
                (
                    Resource.String.prospect_reg_checking_number_title
                    , Resource.String.prospect_reg_checking_number_story
                );
            basicInfo.NextClicked();

        }

        public void CancelClicked()
        {
            basicInfo.CancelClicked();
        }

		public override void OnPause ()
		{
			base.OnPause ();
			basicInfo.CheckPerson -= HandleCheckPerson;
			basicInfo.ValidProspect -= ValidProspect;
			basicInfo.Cancel -= Cancel;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate (Resource.Layout.layout_registration_basic, container, false);

			basicInfo = new BasicInfoView (view.FindViewById<ViewGroup> (Resource.Id.basicInfoFragment), this.Activity);

			if (coordinator.Person != null) {
				basicInfo.SetCustomerDetails (coordinator.Person);
			}

            basicInfo.CheckPerson += HandleCheckPerson;
		    if (basicInfo.NextEventHandlerAlreadySet == false)
		    {
		        basicInfo.ValidProspect += ValidProspect;
		    }
		    basicInfo.Cancel += Cancel;

            coordinator.NavigableFragment = this;
			return view;
		}

        public override void OnDestroy()
        {
            basicInfo.CheckPerson -= HandleCheckPerson;
            basicInfo.ValidProspect -= ValidProspect;
            basicInfo.Cancel -= Cancel;
            base.OnDestroy();
        }

        async void HandleCheckPerson (object sender, CheckPersonEventArgs e)
		{
			if (OwnerActivity.ConnectedToNetwork) {
				//Check for existing prospect or customer
				var solarManager = new SolarManager ();
				var person = await solarManager.CheckPerson (e.PhoneNumber);
				if (person != null) 
				{
					OwnerActivity.HideKeyboard();
					coordinator.onProspectOrCustomerFound (new ProspectCustomerFoundEventArgs (person));
				}
			} else {
				basicInfo.DisplayNetworkRequiredAlert ();
			}
		}

		async void ValidProspect(object sender, EventArgs e)
		{
		    Person person = null;
            try
		    {
                person = await ProspController.GetByPhoneNumberAsync(coordinator.Prospect.Phone);
		        if (person == null && coordinator.ConnectionExists)
		        {
		            person = await ProspController.SearchPersonOnlineAsync(coordinator.Prospect.Phone,OwnerActivity.ConnectedToNetwork);
                    
		        
                    if (person != null && person.Phone != coordinator.Prospect.Phone)
		                //mocking server always returns a prospect regardless of the phone number
		            {
		                person = null;
		            }
		        }
		        if (person != null)
		        {
		            coordinator.ShowAlreadyExistsError(person);
		            return;
		        }
		        coordinator.onProgress(sender);
		    }
		    catch (Exception ex)
		    {
                Toast.MakeText(OwnerActivity, ex.Message,ToastLength.Long).Show();
		        
		    }
		    finally
		    {
		        coordinator.HideWait(person == null ? Result.Ok : Result.Canceled);
		    }
            
            
		}

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (coordinator != null && coordinator.Prospect != null)
            {
                outState.PutString(BundledProspect, JsonConvert.SerializeObject(coordinator.Prospect));
            }
        }

        public bool CancelEnabled
        {
            get
            {
                return view.FindViewById<Button>(Resource.Id.btnCancel)
                    .Enabled;
            }
            set
            {
                view.FindViewById<Button>(Resource.Id.btnCancel)
                    .Enabled = value;
            }
        }

       

        void Cancel(object sender, EventArgs e)
		{
			coordinator.onCancel ();
		}
	}
}

