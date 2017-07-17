using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.ViewModels.Person.Customer;
using SalesApp.Droid.Framework;
using SalesApp.Droid.People.UnifiedUi.Customer;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.Person.Customer
{
    [Activity(Theme = "@style/AppTheme.SmallToolbar", NoHistory = false)]
    public class CustomerPhotoSyncView : MvxViewBase<CustomerPhotoSyncViewModel>
    {
        private const string CustomerPhotoFragTag = "CustomerPhotoFragTag";
        public string NationalId { set; get; }
        public string Phone { set; get; }
        private Button _btnDone;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.customer_photo_sync);
            this.AddToolbar(Resource.String.customer_photo, true);

            if (Intent.Extras != null)
            {
                NationalId = Intent.Extras.GetString("nationalIdKey");
                Phone = Intent.Extras.GetString("customerPhoneKey");
            }

            if (string.IsNullOrEmpty(NationalId))
            {
                throw new Exception("Natonal Id cannot be null");
            }

            this.ViewModel.NationalId = NationalId;
            InitialUi();
        }

        private void InitialUi()
        {
            _btnDone = FindViewById<Button>(Resource.Id.btnDone);
            _btnDone.Click += BtnDoneOnClick; 
            Fragment fragment = SupportFragmentManager.FindFragmentByTag(CustomerPhotoFragTag);
            var fragmentTransaction = GetFragmentManager().BeginTransaction();
            if (fragment == null)
            {
                fragment = new CustomerPhotoFragment();
                Bundle bundle = new Bundle();
                bundle.PutBoolean(CustomerPhotoFragment.InWizardActivity, false);
                bundle.PutString(CustomerPhotoFragment.NationalIdKey, this.ViewModel.NationalId);
                bundle.PutString(CustomerPhotoFragment.PhoneKey, Phone);
                fragment.Arguments = bundle;

                fragmentTransaction.Add(Resource.Id.fragment_holder, fragment, CustomerPhotoFragTag);
            }

            fragmentTransaction.Show(fragment).Commit();
        }

        private async void BtnDoneOnClick(object sender, EventArgs eventArgs)
        {
            await new CustomerPhotoController().UpdateStatus(this.ViewModel.NationalId);
            Intent intent = new Intent();
            Fragment frag = SupportFragmentManager.FindFragmentByTag(CustomerPhotoFragTag);
            CustomerPhotoFragment customerPhotoFragment = (CustomerPhotoFragment) frag;
            List<CustomerPhoto> customerPhotos = customerPhotoFragment.Photos;
            intent.PutExtra(CustomerPhotoFragment.CustomerPhotoFragmentKey, JsonConvert.SerializeObject(customerPhotos));
            SetResult(Result.Ok, intent);
            Finish();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            Logger.Debug("Fragment going in background");
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.ViewModel.CancelCommand.Execute(this.ViewModel);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
 