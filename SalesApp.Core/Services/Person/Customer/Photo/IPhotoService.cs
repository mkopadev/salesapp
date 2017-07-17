using System.Threading.Tasks;

namespace SalesApp.Core.Services.Person.Customer.Photo
{
    public interface IPhotoService
    {
        Task DeletePhotos(string nationalId);
    }
}