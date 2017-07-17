using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Logging;
using Exception = System.Exception;
using String = System.String;

namespace SalesApp.Droid.People.Customers
{
    [Service]
    [IntentFilter(new String[] { "SalesApp.Droid.CustomerPhotoUploadService" })]
    public class CustomerPhotoUploadService : IntentService
    {
        private static readonly ILog Logger = LogManager.Get(typeof(CustomerPhotoUploadService));
        public const string PhotosUploadedAction = "PhotosUploaded";
        public const string PhotoList = "PhotoList";
        public const string PhotoUploadStatusDictionaryString = "PhotoUploadStatusDictionaryString";
        private List<CustomerPhoto> _customerPhotoList;
        private string _photoUploadStatusDictionaryString = string.Empty;

        public CustomerPhotoUploadService() : base("CustomerPhotoUploadService")
        {
        }

        public override IBinder OnBind(Intent intent)
        {
            var binder = new CustomerPhotoUploadServiceBinder(this);
            return null;
        }

        protected override async void OnHandleIntent(Intent intent)
        {
            Logger.Verbose("OnHandleIntent");

            var customerPhotoUploader = new CustomerPhotoUploader();

            if (intent.Extras != null)
            {
                //from the intent get a list of photos to upload
                var photoListString = intent.GetStringExtra(PhotoList);

                if (!string.IsNullOrEmpty(photoListString))
                {
                    Logger.Verbose("OnHandleIntent values retrieved from Intent");
                    Logger.Verbose(photoListString);

                    try
                    {
                        _customerPhotoList = JsonConvert.DeserializeObject<List<CustomerPhoto>>(photoListString);
                        var photoUploadStatusDictionaryResult = await customerPhotoUploader.UploadPhotos(_customerPhotoList);
                        _photoUploadStatusDictionaryString = JsonConvert.SerializeObject(photoUploadStatusDictionaryResult);
                    }
                    catch (JsonException je)
                    {
                        Logger.Error(je);
                    }
                }
            }
            else
            {
                Logger.Verbose("OnHandleIntent NO values retrieved from Intent");

                //if the intent has no extras then upload all pending photos, if any
                try
                {
                    var photoUploadStatusDictionaryResult = await customerPhotoUploader.UploadPhotos();

                    if (photoUploadStatusDictionaryResult != null)
                    {
                        Logger.Verbose(photoUploadStatusDictionaryResult.ToString());
                        _photoUploadStatusDictionaryString =
                            JsonConvert.SerializeObject(photoUploadStatusDictionaryResult);
                    }
                }
                catch (Exception e)
                {
                    Logger.Verbose("OnHandleIntent Exception");
                    Logger.Verbose(e.Message);
                    Logger.Error(e);
                }
            }

            Logger.Verbose("OnHandleIntent value of dictionary string sent to receiver");
            Logger.Verbose(_photoUploadStatusDictionaryString);

            var photosIntent = new Intent(PhotosUploadedAction);
            photosIntent.PutExtra(PhotoUploadStatusDictionaryString, _photoUploadStatusDictionaryString);
            SendOrderedBroadcast(photosIntent, null);
        }
    }

    public class CustomerPhotoUploadServiceBinder : Binder
    {
        protected CustomerPhotoUploadService _photoUploadService;

        public CustomerPhotoUploadService Service
        {
            get { return _photoUploadService; }
        }

        public bool IsBound { get; set; }

        public CustomerPhotoUploadServiceBinder(CustomerPhotoUploadService photoUploadService)
        {
            _photoUploadService = photoUploadService;
        }
    }

    public class CustomerPhotoServiceConnection : Object, IServiceConnection
    {
        public CustomerPhotoServiceConnection()
        {

        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var customerPhotoUploader = new CustomerPhotoUploader();
            customerPhotoUploader.UploadPhotos();
        }

        public void OnServiceDisconnected(ComponentName name)
        {

        }
    }
}