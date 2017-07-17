using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels;

namespace SalesApp.Core.BL.Models.People
{
    public class CustomerPhoto : BusinessEntityBase, IEquatable<CustomerPhoto>
    {
        public string FilePath { get; set; }

        public PhotoType TypeOfPhoto { get; set; }

        public PhotoSaveStatus PhotoStatus { get; set; }

        public PhotoUploadStatus PhotoUploadStatus { get; set; }

        public string CustomerIdentifier { get; set; }

        public string Phone { get; set; }

        public bool UploadFailed
        {
            get
            {
                return this.PhotoUploadStatus == PhotoUploadStatus.Failed || this.PhotoUploadStatus == PhotoUploadStatus.OnHold;
            }
        }

        public string StatusMessage
        {
            get
            {
                switch (this.PhotoUploadStatus)
                {
                        case PhotoUploadStatus.OnHold:
                        case PhotoUploadStatus.Failed:
                            return "Sending has failed";
                        case PhotoUploadStatus.Pending:
                            return "Pending";
                        case PhotoUploadStatus.Successfull:
                            return "Photo has been sent";
                        default:
                            return "Unknown sync status";
                }
            }
        }

        public int StatusIcon
        {
            get
            {
                IDeviceResource deviceResource = Resolver.Instance.Get<IDeviceResource>();

                switch (this.PhotoUploadStatus)
                {
                    case PhotoUploadStatus.Pending:
                        return deviceResource.CustomerPhotoSyncPendingIcon;
                    case PhotoUploadStatus.Successfull:
                        return deviceResource.CustomerPhotoSyncSuccessIcon;
                    case PhotoUploadStatus.OnHold:
                    case PhotoUploadStatus.Failed:
                        return deviceResource.CustomerPhotoSyncFailedIcon;
                    default:
                        return deviceResource.CustomerPhotoSyncFailedIcon;
                }
            }
        }

        public bool Equals(CustomerPhoto other)
        {
            return this.FilePath == other.FilePath;
        }
    }
}
