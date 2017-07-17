using System;
using System.Collections.Generic;
using SalesApp.Core.Enums.People;

namespace SalesApp.Core.Events.CustomerPhoto
{
    public class UploadStatusEventArgs : EventArgs
    {
        public Dictionary<Guid, PhotoUploadStatus> PhotoUploadStatusDictionary { get; private set; }

        public UploadStatusEventArgs(Dictionary<Guid, PhotoUploadStatus> photoUploadStatusDictionary)
        {
            this.PhotoUploadStatusDictionary = photoUploadStatusDictionary;
        }
    }
}
