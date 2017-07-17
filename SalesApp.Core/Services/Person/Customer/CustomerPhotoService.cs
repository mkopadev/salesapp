using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.Services.Person.Customer
{
    public class CustomerPhotoService
    {
        private CustomerPhotoController _controller;

        public CustomerPhotoService()
        {
            this._controller = new CustomerPhotoController();
        }

        public async Task MarkForUpload(List<CustomerPhoto> photos)
        {
            if (photos == null)
            {
                return;
            }

            foreach (var photo in photos)
            {
                photo.PhotoUploadStatus = PhotoUploadStatus.Pending;
            }

            await this._controller.SaveBulkAsync(photos);
        }

        public async Task MarkForUpload(string nationalId)
        {
            string sql = string.Format("UPDATE CustomerPhoto SET PhotoUploadStatus = {0} WHERE CustomerIdentifier = '{1}'", (int)PhotoUploadStatus.Pending, nationalId);

            await DataAccess.Instance.RunQueryAsync(sql);
        }

        public async virtual Task<List<CustomerPhoto>> GetCustomerPhotos(string nationalId)
        {
            CriteriaBuilder criteria = new CriteriaBuilder();
            criteria.Add("CustomerIdentifier", nationalId);

            List<CustomerPhoto> photos = await this._controller.GetManyByCriteria(criteria);

            return photos;
        }

        public async Task<CustomerPhoto> GetMostRecentCustomerPhoto(string nationalId)
        {
            string query = string.Format("SELECT * FROM CustomerPhoto WHERE CustomerIdentifier = '{0}' AND TypeOfPhoto = 1 ORDER BY DateCreated DESC", nationalId);

            List<CustomerPhoto> photos = await new QueryRunner().RunQuery<CustomerPhoto>(query);

            if (photos != null && photos.Count > 0)
            {
                return photos[0];
            }

            return null;
        }
    }
}