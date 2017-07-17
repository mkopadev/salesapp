using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Droid.Framework;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Utils.ViewsHelper;
using SalesApp.Droid.UI.Wizardry;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using File = Java.IO.File;
using Settings = SalesApp.Core.Services.Settings.Settings;
using String = System.String;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    public class CustomerPhotoFragment : WizardStepFragment
    {
        private static ILog _log = LogManager.Get(typeof(CustomerPhotoFragment));
        private const string PhotosDir = "SalesApp";
        private const string StartPointIntentKey = "StartPointIntentKey";
        private const string RegistrationSuccessfulFragment = "RegistrationSuccessfulFragment";
        private const string RegistrationFailedFragment = "RegistrationFailedFragment";
        public const string NationalIdKey = "NationalIdKey";
        public const string CustomerPhotoFragmentKey = "CustomerPhotoFragmentKey";
        public const string PhoneKey = "PhoneKey";

        // Checks whether this fragment is launched from wizard activity or CustomerDetails activity
        public const string InWizardActivity = "InWizardActivity";
        public const string InWizardKey = "InWizardKey";
        private bool _inWizard = true;

        private int _photoWidth;
        private int _photoHeight;
        private const long MemoryLimitThreshold = 100;
        private View _view;
        private SalesApp.Core.BL.Models.People.Customer _personRegistrationInfo;
        private const string BundledRegistrationInfo = "BundledRegistrationInfo";
        private ImageView _addCustomerPhoto, _addDocumentPhoto;
        private ListView _listView;
        private PhotoListAdapter _photoListAdapter;
        private TextView _customerPhotoTextView, _documentPhotoTextView, _titleTextView;
        public CustomerPhotoController CustomerPhotoController;
        private Activity _activity;
        private ViewsHelperUntyped _viewsHelper;
        private DateTime _processingShown;
        private bool _registrationInProgress = false;
        private WizardOverlayFragment _registrationSuccessfulFragment;
        private int _smsFailed = 0;
        private int _maxSmsTries;
        private WizardOverlayFragment _registrationFailedFragment;
        private LinearLayout _customerPhotoLayout;
        private IntentStartPointTracker.IntentStartPoint _startPointIntent;
        private RelativeLayout _selectProductLayout;
        private static Uri _capturedImageUri;
        private const int CaptureImageActivityRequestCodeContentResolver = 101;

        public List<CustomerPhoto> Photos
        {
            get { return _personRegistrationInfo.Photos; }
        }

        public CustomerPhotoController PhotoController
        {
            get
            {
                CustomerPhotoController = CustomerPhotoController ?? new CustomerPhotoController();
                return CustomerPhotoController;
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutJsonObject(BundledRegistrationInfo, _personRegistrationInfo);

            // store the success fragment for redisplay
            if (_registrationSuccessfulFragment != null)
            {
                Activity.SupportFragmentManager.PutFragment(outState, RegistrationSuccessfulFragment, _registrationSuccessfulFragment);
            }

            // store the failed fragment for redisplay
            if (_registrationFailedFragment != null)
            {
                Activity.SupportFragmentManager.PutFragment(outState, RegistrationFailedFragment, _registrationFailedFragment);
            }

            outState.PutJsonObject(StartPointIntentKey, _startPointIntent.ToString());

            outState.PutBoolean(InWizardKey, _inWizard);
        }


        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                _personRegistrationInfo =
                    JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);
                if (_personRegistrationInfo.Product == null)
                {
                    _personRegistrationInfo.Product = new Product();
                }
            }
        }

        public override string GetData()
        {
            return JsonConvert.SerializeObject(_personRegistrationInfo);
        }

        public override Type GetNextFragment()
        {
            return typeof(FragmentCustomerConfirmationScreen);
        }


        public override int NextButtonText
        {
            get
            {
                return Resource.String.next;
            }
        }

        public override int StepTitle
        {
            get { return Resource.String.customer_photo_title; }
        }

        public override bool BeforeGoNext()
        {
            return true;
        }

        public void OnCancel()
        {
            Activity.Finish();
        }

        public override bool Validate()
        {
            if (_personRegistrationInfo.Photos.Count <= 0)
            {
                return false;
            }

            bool valid = false;
            foreach (CustomerPhoto photo in _personRegistrationInfo.Photos)
            {
                if (photo.TypeOfPhoto == PhotoType.Customer)
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this._startPointIntent = ActivityBase.StartPointIntent;
            string nationalId = string.Empty;
            string phone = string.Empty;
            if (this.Arguments != null)
            {
                this._inWizard = this.Arguments.GetBoolean(InWizardActivity, true);
                nationalId = this.Arguments.GetString(NationalIdKey);
                phone = this.Arguments.GetString(PhoneKey);
            }

            if (savedInstanceState != null)
            {
                _personRegistrationInfo =
                      savedInstanceState.GetJsonObject<SalesApp.Core.BL.Models.People.Customer>(BundledRegistrationInfo);

                if (_inWizard)
                {
                    _registrationSuccessfulFragment =
                        Activity.SupportFragmentManager.GetFragment(savedInstanceState, RegistrationSuccessfulFragment)
                            as WizardOverlayFragment;
                    _registrationFailedFragment =
                        Activity.SupportFragmentManager.GetFragment(savedInstanceState, RegistrationFailedFragment) as
                            WizardOverlayFragment;
                    //startPointIntent = savedInstanceState.GetEnum<IntentStartPointTracker.IntentStartPoint>(StartPointIntentKey);
                }
                else
                {
                    _inWizard = savedInstanceState.GetBoolean(InWizardKey);
                }
            }

            if (!_inWizard)
            {
                if (_personRegistrationInfo == null)
                {
                    _personRegistrationInfo = new SalesApp.Core.BL.Models.People.Customer();
                    this._personRegistrationInfo.NationalId = nationalId;
                    this._personRegistrationInfo.Phone = phone;
                }
            }

            _photoWidth = Settings.Instance.CustomerPhotoWidthScale;
            _photoHeight = Settings.Instance.CustomerPhotoHeightScale;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = inflater.Inflate(Resource.Layout.layout_registration_customer_photo, container, false);

            if (savedInstanceState != null && _inWizard)
            {
                string regInfo = savedInstanceState.GetString(BundledRegistrationInfo);
                if (!regInfo.IsBlank())
                {
                    _personRegistrationInfo = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(regInfo);
                }
            }

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Customer Photo");

            _addCustomerPhoto = this.FragmentView.FindViewById<ImageView>(Resource.Id.imageView_add_customer_photo);
            _addDocumentPhoto = this.FragmentView.FindViewById<ImageView>(Resource.Id.imageView_add_document_photo);
            _customerPhotoTextView = this.FragmentView.FindViewById<TextView>(Resource.Id.textView_customer_photo);
            _documentPhotoTextView = this.FragmentView.FindViewById<TextView>(Resource.Id.textView_document_photo);
            _titleTextView = this.FragmentView.FindViewById<TextView>(Resource.Id.textView_select_product);
            _listView = this.FragmentView.FindViewById<ListView>(Resource.Id.listView_photos);
            _customerPhotoLayout = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.customer_photo_layout);
            _selectProductLayout = this.FragmentView.FindViewById<RelativeLayout>(Resource.Id.select_product_layout);
            View buttonDivider = this.FragmentView.FindViewById(Resource.Id.bottom_separator);
            _photoListAdapter = new PhotoListAdapter(_inWizard, Photos, _activity, this);
            _listView.Adapter = _photoListAdapter;

            if (_inWizard)
            {
                WizardActivity.ButtonNext.Text = GetString(Resource.String.next);
                WizardActivity.ButtonNext.Visibility = ViewStates.Visible;
                buttonDivider.Visibility = ViewStates.Gone;
            }
            else
            {
                Activity.SetTitle(Resource.String.customer_photo_title);
                _selectProductLayout.Visibility = ViewStates.Gone;
                _customerPhotoTextView.Text = GetString(Resource.String.add_customer_photo);
                _documentPhotoTextView.Text = GetString(Resource.String.add_document_photo);
                buttonDivider.Visibility = ViewStates.Visible;
            }

            RefreshList();
            _addCustomerPhoto.Click += delegate
            {
                _personRegistrationInfo.TypeOfPhotoBeingTaken = PhotoType.Customer;
                TakePhoto();
            };

            _addDocumentPhoto.Click += delegate
            {
                _personRegistrationInfo.TypeOfPhotoBeingTaken = PhotoType.Document;
                TakePhoto();
            };

            return this.FragmentView;
        }

        public async Task RefreshList()
        {
            DateTime today = DateTime.Today;

            if (_personRegistrationInfo.Photos == null)
            {
                return;
            }

            if (_personRegistrationInfo.Photos.Count <= 0)
            {            
                _personRegistrationInfo.Photos = await PhotoController.GetPhotosTakenOnDate(today, _personRegistrationInfo.NationalId, _inWizard);
            }

            int count = _photoListAdapter.Count;

            if (_titleTextView == null)
            {
                return;
            }

            _titleTextView.Text = count > 0 ? GetString(Resource.String.add_another_photo) : GetString(Resource.String.select_photo_type);

            if (_inWizard)
            {
                if (Validate())
                {
                    WizardActivity.ButtonNext.Enabled = true;
                    if (_customerPhotoLayout != null)
                    {
                        _customerPhotoLayout.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    WizardActivity.ButtonNext.Enabled = false;
                    if (_customerPhotoLayout != null)
                    {
                        _customerPhotoLayout.Visibility = ViewStates.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// create the photos repository
        /// </summary>
        private File CreateDirectoryForPhotos()
        {
            _log.Verbose("Creating a directory for photos");
            var photosDir = new File(Environment.ExternalStorageDirectory, PhotosDir);

            if (!photosDir.Exists())
            {
                photosDir.Mkdirs();
                _log.Verbose("Created directory for photos");
            }

            return photosDir;
        }

        private void TakePhoto()
        {
            if (IsThereAnAppToTakePictures())
            {

                _capturedImageUri = Activity.ContentResolver.Insert(MediaStore.Images.Media.ExternalContentUri,
                    new ContentValues());

                _log.Verbose("URI Photo path " + _capturedImageUri.Path);

                Intent cameraIntent = new Intent(MediaStore.ActionImageCapture);
                cameraIntent.PutExtra(MediaStore.ExtraOutput, _capturedImageUri);
                StartActivityForResult(cameraIntent, CaptureImageActivityRequestCodeContentResolver);
            }
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._activity = activity;
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            ICursor cursor = null;
            try
            {
                File photoFile = null;
                _log = LogManager.Get(typeof (CustomerPhotoFragment));

                if (requestCode == CaptureImageActivityRequestCodeContentResolver)
                {
                    if (resultCode == (int) Result.Ok)
                    {
                        String[] projection =
                        {
                            MediaStore.MediaColumns.Id,
                            MediaStore.Images.ImageColumns.Orientation,
                            MediaStore.Images.Media.InterfaceConsts.Data
                        };

                        cursor = Activity.ContentResolver.Query(_capturedImageUri, projection, null, null, null);
                        if (cursor != null)
                        {
                            cursor.MoveToFirst();
                            var photoFileName = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data));

                            // Get filename generated from contentresolver and create a new File object from it
                            photoFile = new File(photoFileName);
                        }

                        if (photoFile != null)
                        {
                             await HandleCameraRequest(photoFile);
                        }
                        else
                        {
                            FailedTakingPhoto();
                        }

                    }
                    else if (resultCode == (int) Result.Canceled)
                    {
                        Toast.MakeText(_activity, GetString(Resource.String.image_capture_cancelled), ToastLength.Long).Show();
                    }
                    else
                    {
                        FailedTakingPhoto();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                FailedTakingPhoto();
            }
            finally
            {
               if (cursor != null)
                {
                    cursor.Close();
                }
            }
        }

        /*private File PersistBitmap(Bitmap bitmap)
        {
            //save a new file to the predefined location
            File photosDir = CreateDirectoryForPhotos();
            if (photosDir == null)
            {
                return null;
            }

            // Changed compression format to png because png doesnt loose quality on compression
            File photoFile = new File(photosDir, String.Format("photo_{0}.jpg", Guid.NewGuid()));
            var stream = new FileStream(photoFile.AbsolutePath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            stream.Close();
            return photoFile;
        }

        private string GetRealPathFromURI(Uri uri)
        {
            ICursor cursor = null;
            try
            {
                string path = null;

                // The projection contains the columns we want to return in our query.
                ContentResolver contentResolver = _activity.ContentResolver;
                if (contentResolver == null)
                {
                    return null;
                }
                string[] projection = new[] {MediaStore.Audio.Media.InterfaceConsts.Data};

                cursor = contentResolver.Query(uri, projection, null, null, null);

                if (cursor != null)
                {
                    int columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(columnIndex);
                }

                using (ICursor cursor = contentResolver.Query(uri, projection, null, null, null))
                {
                    if (cursor != null)
                    {
                        int columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data);
                        cursor.MoveToFirst();
                        path = cursor.GetString(columnIndex);
                    }
                }

                return path;
            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                }
            }
        }*/

        private void FailedTakingPhoto()
        {
            _log.Verbose("On OnActivityResult - Oh crap -- Saving file failed");
            Toast.MakeText(_activity, GetString(Resource.String.photo_saving_failed), ToastLength.Long).Show();
        }

        private async Task HandleCameraRequest(File file)
        {
            _log.Verbose("HandleCameraRequest");

            string path = null;
            try
            {
                // Resize the image
                path = LoadAndResizeBitmap(file, _photoWidth, _photoHeight);
            }
            catch (OutOfMemoryError e)
            {
                GC.Collect();
                _log.Verbose("Oops, Got an OutOfMemoryError when loading image");
                _log.Error(e);
            }
            catch (Exception e)
            {
                _log.Verbose("Oops, Got an Exception when loading image");
                _log.Error(e);
            }

            if (path != null)
            {
                _log.Verbose("Created bitmap successfully");

                CustomerPhoto photo = new CustomerPhoto
                {
                    FilePath = path,
                    TypeOfPhoto = _personRegistrationInfo.TypeOfPhotoBeingTaken,
                    PhotoStatus = PhotoSaveStatus.Successful,
                    PhotoUploadStatus = PhotoUploadStatus.OnHold,
                    CustomerIdentifier = _personRegistrationInfo.NationalId,
                    Phone = _personRegistrationInfo.Phone
                };

                // saving to db only if the photo file actually exists
                await PersistCustomerPhoto(photo);
                this._photoListAdapter.AddItem(photo);
            }
            else
            {
                _log.Verbose("Oh crap, Creating bitmap failed");
                FailedTakingPhoto();
            }
        }

        private async Task PersistCustomerPhoto(CustomerPhoto customerPhoto)
        {
            await PhotoController.SaveAsync(customerPhoto);

            _personRegistrationInfo.Photos.Add(customerPhoto);
            this.RefreshList();
        }

        /// <summary>
        /// check if there is an app to take photos
        /// </summary>
        /// <returns></returns>
        private bool IsThereAnAppToTakePictures()
        {
            _log.Verbose("Checking if there is an app to take photos");
            PackageManager packageManager = _activity.PackageManager;
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
               packageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        /// <summary>
        /// resizing bitmaps
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private string LoadAndResizeBitmap(File file, int width, int height)
        {
            File photoFile = null;
            string path = null;

            _log.Verbose("Resizing bitmap to " + width + " by " + height);
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(file.AbsolutePath, options);

            // calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(file.AbsolutePath, options);

            //save a new file to the predefined location
            File photosDir = CreateDirectoryForPhotos();
            if (photosDir == null)
            {
                return null;
            }

            photoFile = new File(photosDir, String.Format("photo_{0}.jpg", Guid.NewGuid()));
            var stream = new FileStream(photoFile.AbsolutePath, FileMode.Create);
            resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            stream.Close();

            Logger.Verbose("New Photo path " + photoFile.AbsolutePath);
            //Logger.Verbose("New Photo Totalspace " + photoFile.TotalSpace);
            Logger.Verbose("New Photo Length " + photoFile.Length());

            _log.Verbose("Saved the photo file in a different location. Delete the original one");
            file.Delete();
            _log.Verbose("Deleted the original");

            path = photoFile.AbsolutePath;

            //dispose unwanted objects
            options = null;
            resizedBitmap = null;
            photosDir = null;
            GC.Collect();

            return path;
        }

        public void RemovePhoto(CustomerPhoto photo)
        {
            if (this._personRegistrationInfo.Photos == null)
            {
                return;
            }

            this._personRegistrationInfo.Photos.Remove(photo);
        }

        /*public static async void DeleteOldPhotos(int maxPhotosToDelete)
        {
            CustomerPhotoController customerPhotoController = new CustomerPhotoController();
            List<CustomerPhoto> oldPhotos = customerPhotoController.GetOldestPhotos(maxPhotosToDelete);
            foreach (var photo in oldPhotos)
            {
                _log.Verbose("Photo " + photo.FilePath);
                File file = new File(photo.FilePath);
                if (file.Exists())
                {
                    file.Delete();
                }
                await customerPhotoController.DeleteAsync(photo);
            }
        }


        public static Boolean ShouldDeleteOldPhotos()
        {
            if (ExternalMemoryAvailable())
            {
                long availableExternalMem = GetAvailableExternalMemorySize();
                return availableExternalMem <= MemoryLimitThreshold;
            }
            else
            {
                long availableInternalMem = GetAvailableInternalMemorySize();
                return availableInternalMem <= MemoryLimitThreshold;
            }
        }

        public static long GetAvailableInternalMemorySize()
        {
            File path = Environment.DataDirectory;
            StatFs stat = new StatFs(path.Path);
            long blockSize = stat.BlockSize;
            long availableBlocks = stat.AvailableBlocks;
            long availableInternalMem = availableBlocks * blockSize / 1024 / 1024;
            _log.Verbose("availableInternalMem in Mb " + availableInternalMem);
            return availableInternalMem;
        }

        public static long GetAvailableExternalMemorySize()
        {
            long availableExternalMem = 0;
            if (ExternalMemoryAvailable())
            {
                File path = Environment.ExternalStorageDirectory;
                StatFs stat = new StatFs(path.Path);
                long blockSize = stat.BlockSize;
                long availableBlocks = stat.AvailableBlocks;
                availableExternalMem = availableBlocks * blockSize / 1024 / 1024;
            }
            _log.Verbose("availableInternalMem in Mb " + availableExternalMem);
            return availableExternalMem;
        }

        public static Boolean ExternalMemoryAvailable()
        {
            return Environment.ExternalStorageState.Equals(
                    Environment.MediaMounted);
        }

        public static DateTime EndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static DateTime StartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }
        
        public async Task DeleteOnCancel()
        {
            await PhotoController.DeleteOnCancel(_personRegistrationInfo.NationalId);
        }

        public async Task UpdateStatus()
        {
            await PhotoController.UpdateStatus(_personRegistrationInfo.NationalId);
        }*/
    }
}

