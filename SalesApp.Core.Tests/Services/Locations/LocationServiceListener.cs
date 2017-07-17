using System.Threading.Tasks;
using SalesApp.Core.Services.Locations;

namespace SalesApp.Core.Tests.Services.Locations
{
    public class LocationServiceListener : ILocationServiceListener
    {
        public async Task<string> GetLocation()
        {
            return "0+0";
        }

        public void StartLocationUpdate()
        {
            // do nothing
        }

        public bool IsLocationOn()
        {
            return false;
        }
    }
}