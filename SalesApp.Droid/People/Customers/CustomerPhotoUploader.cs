using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Droid.People.Customers
{
    public class CustomerPhotoUploader
    {
        private string _azureSasToken ;
        private int _uploadRetryCount;
        private TimeSpan _uploadRetryDuration;
        private CloudBlobContainer _photoBlobContainer;
        private BlobRequestOptions _blobRequestOptions;
        private CustomerPhotoController _photoController;
        private static readonly ILog Logger = LogManager.Get(typeof(CustomerPhotoUploader));
        private Context _context = Application.Context;
        private IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();

        public CustomerPhotoUploader()
        {
            Initialize();
        }

        private void Initialize()
        {

            try
            {
                Logger.Verbose("Retrieving Azure SAS Token from OTA");

                //retrieve windows azure sas token from OTA
                _azureSasToken = Settings.Instance.AzureSasToken;

                //if the azure token key is empty then get default values defined in the string resource file, depending on the environment and or country
                if (string.IsNullOrEmpty(_azureSasToken))
                {
                    Logger.Verbose("Azure Token is Empty, getting default value from string file");
#if DEBUG || UAT
                    Logger.Verbose("Retrieving UAT Azure SAS Token default value");
                    _azureSasToken = _context.GetString(Resource.String.azure_uat_token);
#elif STAGING
                 Logger.Verbose("Retrieving STAGING Azure SAS Token default value");
                _azureSasToken = _context.GetString(Resource.String.azure_staging_token);
#else
                switch (Settings.Instance.DsrCountryCode)
                {
                    case CountryCodes.KE:
                         Logger.Verbose("Retrieving RELEASE KE Azure SAS Token default value");
                        _azureSasToken = _context.GetString(Resource.String.azure_ke_prod_token);
                        break;
                    case CountryCodes.UG:
                         Logger.Verbose("Retrieving RELEASE UG Azure SAS Token default value");
                        _azureSasToken = _context.GetString(Resource.String.azure_ug_prod_token);
                        break;
                    case CountryCodes.TZ:
                         Logger.Verbose("Retrieving RELEASE TZ Azure SAS Token default value");
                        _azureSasToken = _context.GetString(Resource.String.azure_tz_prod_token);
                        break;
                    case CountryCodes.GH:
                         Logger.Verbose("Retrieving RELEASE GH Azure SAS Token default value");
                        _azureSasToken = _context.GetString(Resource.String.azure_gh_prod_token);
                        break;
                }
#endif                    
                }

                Logger.Verbose(_azureSasToken);

                _uploadRetryCount = 3;
                _uploadRetryDuration = TimeSpan.FromSeconds(5);

                //For large blobs, images above 1MB are uploaded in blocks in another thread
                _blobRequestOptions = new BlobRequestOptions()
                {
                    SingleBlobUploadThresholdInBytes = 1024*1024, //1MB, the minimum
                    ParallelOperationThreadCount = 1,
                    //ServerTimeout = TimeSpan.FromSeconds(20),
                    //RetryPolicy = new ExponentialRetry(backOffPeriod, retryCount),
                    RetryPolicy = new LinearRetry(_uploadRetryDuration, _uploadRetryCount)
                };

                _photoBlobContainer = new CloudBlobContainer(new Uri(_azureSasToken));
                _photoBlobContainer.ServiceClient.DefaultRequestOptions = _blobRequestOptions;

                _photoController = new CustomerPhotoController();
            }
            catch (StorageException se)
            {
                Logger.Verbose("StorageException when Initializing CloudBlobContainer");
                Logger.Verbose(se.Message);
                Logger.Error(se);
            }
            catch (Exception e)
            {
                Logger.Verbose("General Exception when Initializing CloudBlobContainer, SAS Token is Empty");
                Logger.Verbose(e.Message);
                Logger.Error(e);
            }
        }

        public async Task<Dictionary<Guid, PhotoUploadStatus>> UploadPhoto(CustomerPhoto customerPhoto)
        {
            //add the passed photo
            var photos = new List<CustomerPhoto> {customerPhoto};
            var photoUploadStatusDict = await Upload(photos);

            return photoUploadStatusDict;
        }

        public async Task<Dictionary<Guid, PhotoUploadStatus>> UploadPhotos(List<CustomerPhoto> photos)
        {
            //add the passed photos
            var photoUploadStatusDict =  await Upload(photos);

            return photoUploadStatusDict;
        }

        public async Task<Dictionary<Guid, PhotoUploadStatus>> UploadPhotos()
        {
            //get all photos with PhotoUploadStatus = Pending or Failed
            var photos = await _photoController.GetUploadableAsync();
            var photoUploadStatusDict = await Upload(photos);

            return photoUploadStatusDict;
        }

        private async Task<Dictionary<Guid, PhotoUploadStatus>> Upload(List<CustomerPhoto> photos)
        {
            var photoUploadStatusDict = new Dictionary<Guid, PhotoUploadStatus>();
            CustomerPhoto currentPhotoForUpload = null;
            PhotoUploadStatus photoUploadStatus = PhotoUploadStatus.Pending;

            try
            {
                if (_connectivityService.HasConnection() && photos.Any())
                {
                    Logger.Verbose("Total number of Photos available " + photos.Count);

                    // loop through existing customer photos, if any
                    foreach (var photo in photos)
                    {
                        currentPhotoForUpload = photo;
                        photoUploadStatus = photo.PhotoUploadStatus;

                        var photoName = Path.GetFileName(currentPhotoForUpload.FilePath);
                        Logger.Verbose("About to process Photo : " + photoName);

                        Logger.Verbose("DSR Country Code");
                        Logger.Verbose(Settings.Instance.DsrCountryCode.ToString().ToLower());

                        // build up a blob name that includes the phone and national id number of the customer as well as country prefix
                        var blobName = currentPhotoForUpload.Phone + "_" + currentPhotoForUpload.CustomerIdentifier + "_" + Settings.Instance.DsrCountryCode.ToString().ToLower() + "_" + Path.GetFileName(photoName);
                        var blockBlob = _photoBlobContainer.GetBlockBlobReference(Path.GetFileName(blobName));

                        // set the metadata of the blob to have customer's national id, type and description of photo
                        if (!string.IsNullOrEmpty(currentPhotoForUpload.CustomerIdentifier))
                        {
                            blockBlob.Metadata["NationalID"] = currentPhotoForUpload.CustomerIdentifier;
                        }

                        if (!string.IsNullOrEmpty(currentPhotoForUpload.Phone))
                        {
                            blockBlob.Metadata["PhoneNumber"] = currentPhotoForUpload.Phone;
                        }

                        if (currentPhotoForUpload.TypeOfPhoto > 0)
                        {
                            blockBlob.Metadata["PhotoType"] = currentPhotoForUpload.TypeOfPhoto.ToString();
                        }

                        // upload each photo to Azure Blob Storage
                        if (File.Exists(currentPhotoForUpload.FilePath))
                        {
                            using (var fileStream = File.OpenRead(currentPhotoForUpload.FilePath))
                            {
                                await blockBlob.UploadFromStreamAsync(fileStream);
                                Logger.Verbose("Photo uploaded to Azure Blob Storage : " + blobName);
                            }
                        }

                        // set the upload status to success
                        photoUploadStatus = PhotoUploadStatus.Successfull;

                        // save the photo's status to the database
                        currentPhotoForUpload.PhotoUploadStatus = photoUploadStatus;
                        await _photoController.SaveAsync(currentPhotoForUpload);

                        // update the photo status dictionary
                        photoUploadStatusDict.Add(currentPhotoForUpload.Id, currentPhotoForUpload.PhotoUploadStatus);

                        Logger.Verbose("Updated Customer Photo Status to Success");
                    }
                }
            }
            catch (StorageException se)
            {
                Logger.Verbose("StorgeException Caught On Photo Upload");

                // set the upload status to Fail
                photoUploadStatus = PhotoUploadStatus.Failed;

                var requestInformation = se.RequestInformation;
                var errorCode = requestInformation.ExtendedErrorInformation.ErrorCode;
                var statusCode = (HttpStatusCode)requestInformation.HttpStatusCode;

                Logger.Verbose(errorCode);
                Logger.Verbose(statusCode.ToString());

                // if it is an authentication failure then it means the SAS Token has probably expired and a new one is needed
                if (errorCode == StorageErrorCodeStrings.AuthenticationFailed)
                {
                    // SAS Token probably has expired
                    Logger.Verbose("SAS Token Expired");

                    // TODO Add code to request new SAS Token from OTA Settings or API Call
                }
            }
            catch (Exception e)
            {
                Logger.Verbose("General Exception Caught On Photo Upload");
                Logger.Verbose(e.Message);

                // set the upload status to Fail
                photoUploadStatus = PhotoUploadStatus.Failed;

                Logger.Error(e);
            }

            try
            {

                // if the upload failed, update the database accordingly, but from another try/catch block
                if (photoUploadStatus == PhotoUploadStatus.Failed && currentPhotoForUpload != null)
                {
                    currentPhotoForUpload.PhotoUploadStatus = photoUploadStatus;
                    await _photoController.SaveAsync(currentPhotoForUpload);

                    // update the photo status dictionary
                    photoUploadStatusDict.Add(currentPhotoForUpload.Id, currentPhotoForUpload.PhotoUploadStatus);

                    Logger.Verbose("Updated Customer Photo Status to Failed");
                }
            }
            catch (Exception dbe)
            {
                Logger.Verbose("General Exception Caught In CustomerPhoto Table Updated on Failed Upload");
                Logger.Verbose(dbe);
                Logger.Error(dbe);
            }

            return photoUploadStatusDict;
        }
    }
}