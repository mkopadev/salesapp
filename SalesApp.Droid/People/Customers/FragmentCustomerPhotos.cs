using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Events.CustomerPhoto;
using SalesApp.Core.Services.Person.Customer;
using SalesApp.Core.ViewModels.Person.Customer;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.People.UnifiedUi.Customer;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Stats;
using SalesApp.Droid.Views.Person.Customer;

namespace SalesApp.Droid.People.Customers
{
    public class FragmentCustomerPhotos : MvxFragmentBase, ISwipeRefreshFragment
    {
        public const string CustomerNationalIdBundleKey = "CustomerNationalIdBundleKey";
        public const string CustomerPhoneBundleKey = "CustomerPhoneBundleKey";
        private CustomerPhotoUploaderReceiver _customerPhotoUploaderReceiver;
        private string _customerNationalId;
        private string _phone;
        private Button _addPhoto;
        public const int AddPhotoResult = 1;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_customer_photos, null);
            _addPhoto = this.FragmentView.FindViewById<Button>(Resource.Id.add_photo);
            var adapter = new CustomerPhotoListAdapter(this.Activity, (IMvxAndroidBindingContext)this.BindingContext);

            MvxListView photoList = this.FragmentView.FindViewById<MvxListView>(Resource.Id.photo_list);
            photoList.Adapter = adapter;

            if (this.Arguments != null)
            {
                _customerNationalId = this.Arguments.GetString(CustomerNationalIdBundleKey);
                _phone = this.Arguments.GetString(CustomerPhoneBundleKey);
            }

            CustomerPhotoViewModel viewModel = new CustomerPhotoViewModel(_customerNationalId, new CustomerPhotoService());
            this.ViewModel = viewModel;

            adapter.PhotoUpdated += AdapterOnPhotoUpdated;
            _customerPhotoUploaderReceiver = new CustomerPhotoUploaderReceiver();

            _addPhoto.Click += AddPhotoOnClick;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.customer_photo));

            return this.FragmentView;
        }

        private void AddPhotoOnClick(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(Activity, typeof(CustomerPhotoSyncView));
            intent.PutExtra("nationalIdKey", _customerNationalId);
            intent.PutExtra("customerPhoneKey", _phone);
            StartActivityForResult(intent, AddPhotoResult);
        }

        private async void AdapterOnPhotoUpdated(object sender, CustomerPhotoUpdatedEvent e)
        {
            CustomerPhotoViewModel vm = this.ViewModel as CustomerPhotoViewModel;
            if (vm == null)
            {
                return;
            }
            
            vm.PhotoUpdated(e.Position);

            // serialize the selected photo and pass it's string to the Photo Upload service
            CustomerPhoto photo = vm.CustomerPhotos[e.Position];
            var photoList = new List<CustomerPhoto> { photo };
            var photoListString = JsonConvert.SerializeObject(photoList);

            Logger.Verbose(photoListString);

            var photoUploadServiceIntent = new Intent(Activity, typeof(CustomerPhotoUploadService));
            photoUploadServiceIntent.PutExtra(CustomerPhotoUploadService.PhotoList, photoListString);

            Activity.StartService(photoUploadServiceIntent);
        }

        public override void OnResume()
        {
            base.OnResume();

            // register to get upload statuses from the photo upload service
            var intentFilter = new IntentFilter(CustomerPhotoUploadService.PhotosUploadedAction) { Priority = (int)IntentFilterPriority.HighPriority };
            Activity.RegisterReceiver(_customerPhotoUploaderReceiver, intentFilter);

            _customerPhotoUploaderReceiver.UploadStatusEvent += UploadStatusEvent;
        }

        public override void OnPause()
        {
            base.OnPause();
            Activity.UnregisterReceiver(_customerPhotoUploaderReceiver);
        }

        private void UploadStatusEvent(object sender, UploadStatusEventArgs e)
        {
            CustomerPhotoViewModel vm = this.ViewModel as CustomerPhotoViewModel;
            if (e.PhotoUploadStatusDictionary == null || vm == null)
            {
                return;
            }

            foreach (var map in e.PhotoUploadStatusDictionary)
            {
                CustomerPhoto photo = new CustomerPhoto
                {
                    Id = map.Key,
                    PhotoUploadStatus = map.Value
                };

                vm.PhotoUpdated(photo);
            }
        }

        public async Task SwipeRefresh(bool forceRemote)
        {
            CustomerPhotoViewModel vm = this.GetTypeSafeViewModel<CustomerPhotoViewModel>();
            vm.GetCustomerPhotos();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (data != null)
            {
                string customerPhotos = data.Extras.GetString(CustomerPhotoFragment.CustomerPhotoFragmentKey);
                if (!string.IsNullOrWhiteSpace(customerPhotos))
                {
                    var photos = JsonConvert.DeserializeObject<List<CustomerPhoto>>(customerPhotos);
                    var vm = this.GetTypeSafeViewModel<CustomerPhotoViewModel>();
                    vm.UpdatePhotos(photos);
                }
            }
        }
    }
}