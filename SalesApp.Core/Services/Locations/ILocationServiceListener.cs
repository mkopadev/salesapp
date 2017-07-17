using System.Threading.Tasks;

namespace SalesApp.Core.Services.Locations
{
    public interface ILocationServiceListener
    {
        Task<string> GetLocation();

        void StartLocationUpdate();

        bool IsLocationOn();
    }
}
