using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using SalesApp.Core.BL.Cache;
using SalesApp.Core.Services.Locations;

namespace SalesApp.Droid.Services.Locations
{
    public class LocationService : Object, ILocationServiceListener, ILocationListener
    {
        private LocationManager _locationManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
        private MemoryCache _cache = MemoryCache.Instance;
        private string _provider;

        public async Task<string> GetLocation()
        {
            string currentLocation = "0+0";

            LocationUpdate locUpdate = await GetLocatioUpdate();

            if (locUpdate != null && locUpdate.Value != null)
            {
                currentLocation = locUpdate.Value;
            }

           return currentLocation;
        }

        private async Task<LocationUpdate> GetLocatioUpdate()
        {
            LocationUpdate t = await _cache.Get<LocationUpdate>("location");
            return t;
        }

        public void StartLocationUpdate()
        {
            if (!IsLocationOn())
            {
                return;
            }

            this._provider = LocationManager.NetworkProvider;

            _locationManager.RequestLocationUpdates(this._provider, 2000, 0, this);
            Location location = _locationManager.GetLastKnownLocation(this._provider);
            SaveBestLocation(location);
        }

        public bool IsLocationOn()
        {
            if (!_locationManager.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                return false;
            }

            return true;
        }

        public void OnLocationChanged(Location location)
        {
            SaveBestLocation(location);
        }

        private void SaveBestLocation(Location location)
        {
            if (location != null)
            {
            var latitude = Location.Convert(location.Latitude, Format.Degrees);
            var longitude = Location.Convert(location.Longitude, Format.Degrees);
            double longitudeD = double.Parse(longitude, CultureInfo.InvariantCulture);
            double latitudeD = double.Parse(latitude, CultureInfo.InvariantCulture);
            string currentLoc = latitudeD.ToString("##.####") + string.Empty + (longitudeD < 0 ? longitudeD.ToString("###.####") : "+" + longitudeD.ToString("###.####"));
            LocationUpdate locUpdate = new LocationUpdate { Value = currentLoc };
                _cache.Store("location", locUpdate);
            _locationManager.RemoveUpdates(this);
        }
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }
    }
}