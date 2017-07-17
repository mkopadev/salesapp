using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.People
{
    public class CustomerPhotoController : SQLiteDataService<CustomerPhoto>
    {
        public async override Task<List<CustomerPhoto>> GetAllAsync()
        {
            List<CustomerPhoto> customerPhotos = await base.GetAllAsync();
            return customerPhotos.OrderByDescending(customerPhoto => customerPhoto.Created).ToList();
        }

        public async Task<List<CustomerPhoto>> GetUploadableAsync()
        {
            string sql = "SELECT * FROM CustomerPhoto WHERE PhotoUploadStatus = 2 OR PhotoUploadStatus = 4";
            List<CustomerPhoto> customerPhotos = await new QueryRunner().RunQuery<CustomerPhoto>(sql);

            return customerPhotos;
        }

        public override List<CustomerPhoto> GetAll()
        {
            List<CustomerPhoto> customerPhotos = base.GetAll();
            return customerPhotos.OrderByDescending(customerPhoto => customerPhoto.Created).ToList();
        }

        public List<CustomerPhoto> GetOldestPhotos(int limit)
        {
            List<CustomerPhoto> customerPhotos = base.GetAll();
            return customerPhotos.OrderBy(customerPhoto => customerPhoto.Created).Take(limit).ToList();
        }

        public List<CustomerPhoto> GetPhotosTakenOnDate(DateTime date)
        {
            List<CustomerPhoto> customerPhotos = base.GetAll();
            return customerPhotos
                .Where(customerPhoto => customerPhoto.Created > date)
                .OrderBy(customerPhoto => customerPhoto.Created)
                .ToList();
        }

        public async Task<List<CustomerPhoto>> GetPhotosTakenOnDate(DateTime date, string nationalId, bool inWizard)
        {
            string sql = "SELECT * FROM CustomerPhoto WHERE CustomerIdentifier = '" + nationalId + "' ";
            if (!inWizard)
            {
                sql += " AND  PhotoUploadStatus = '1' ";
            }
            else
            {
                sql += " AND date(DateCreated) >= date('now')";
            }

            Logger.Debug("CustomerPhoto " + sql);

            List<CustomerPhoto> customerPhotos = await new QueryRunner().RunQuery<CustomerPhoto>(sql);

            return customerPhotos;
        }

        public async override Task<SaveResponse<CustomerPhoto>> SaveAsync(CustomerPhoto model)
        {
                return await base.SaveAsync(model);
            }

        public async Task UpdateStatus(string nationalId)
        {
            await DataAccess.Instance.RunQueryAsync("UPDATE CustomerPhoto SET PhotoUploadStatus = 2 WHERE CustomerIdentifier = '" + nationalId + "' AND PhotoUploadStatus = 1");
        }

        public async Task<int> GetPhotos(string nationalId)
        {
            CriteriaBuilder builder = new CriteriaBuilder();
            builder.Add("CustomerIdentifier", nationalId);
            builder.Add("PhotoUploadStatus", 1);
            List<CustomerPhoto> photos = await GetManyByCriteria(builder);
            return photos.Count;
        }
    }
}
