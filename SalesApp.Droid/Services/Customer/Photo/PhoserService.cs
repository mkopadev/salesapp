using System.Collections.Generic;
using System.Threading.Tasks;
using Java.IO;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.Person.Customer.Photo;

namespace SalesApp.Droid.Services.Customer.Photo
{
    public class PhotoService : IPhotoService
    {
        public async Task DeletePhotos(string nationalId)
        {
            CriteriaBuilder builder = new CriteriaBuilder();
            builder.Add("CustomerIdentifier", nationalId);
            builder.Add("PhotoUploadStatus", "1");
            List<CustomerPhoto> customerPhotos = await new CustomerPhotoController().GetManyByCriteria(builder);
            foreach (var photo in customerPhotos)
            {
                File file = new File(photo.FilePath);
                if (file.Exists())
                {
                    file.Delete();
                }

                new CustomerPhotoController().DeleteAsync(photo);
            }
        }
    }
}