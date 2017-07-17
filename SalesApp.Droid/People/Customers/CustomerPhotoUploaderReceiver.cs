using System;
using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Events.CustomerPhoto;
using SalesApp.Core.Logging;

namespace SalesApp.Droid.People.Customers
{
    [BroadcastReceiver]
    public class CustomerPhotoUploaderReceiver : BroadcastReceiver
    {
        private static readonly ILog Logger = LogManager.Get(typeof(CustomerPhotoUploaderReceiver));

        public event EventHandler<UploadStatusEventArgs> UploadStatusEvent;

        public override void OnReceive(Context context, Intent intent)
        {
            Logger.Verbose("OnReceive");

            // receive the upload status(es) of photo(s)
            if (intent.Extras != null)
            {
                // from the intent get a list of photos to upload
                var photoUploadStatusDictionaryString = intent.GetStringExtra(CustomerPhotoUploadService.PhotoUploadStatusDictionaryString);

                Logger.Verbose("Upload status received string");
                Logger.Verbose(photoUploadStatusDictionaryString);

                if (!string.IsNullOrEmpty(photoUploadStatusDictionaryString))
                {
                    try
                    {
                        var photoUploadStatusDictionary = JsonConvert.DeserializeObject<Dictionary<Guid, PhotoUploadStatus>>(photoUploadStatusDictionaryString);
                        Logger.Verbose("Received upload status count : " + photoUploadStatusDictionary.Count);

                        if (UploadStatusEvent != null)
                        {
                            UploadStatusEvent.Invoke(this, new UploadStatusEventArgs(photoUploadStatusDictionary));
                        }
                    }
                    catch (JsonException je)
                    {
                        Logger.Error(je);
                    }
                }
            }
        }
    }
}