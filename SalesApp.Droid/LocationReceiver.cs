using Android.Content;
using SalesApp.Core.Services.Locations;
using SalesApp.Droid.Services.Locations;

namespace SalesApp.Droid
{
    [BroadcastReceiver]
    public class LocationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            RequestLocation();
        }

        private void RequestLocation()
        {
            ILocationServiceListener locationService = new LocationService();
            locationService.StartLocationUpdate();
        }
    }
}