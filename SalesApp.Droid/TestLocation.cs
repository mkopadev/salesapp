using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MK.Solar
{
    [Service]
    [IntentFilter(new String[] { "MKopa" })]
    public class TestLocation : IntentService,ILocationListener
    {
        private LocationManager locationManager = Android.App.Application.Context.GetSystemService(Context.LocationService) as LocationManager;

        public void OnLocationChanged(Location location)
        {
            Console.WriteLine("Location is " + location.Latitude + " " + location.Longitude);
        }

        public void OnProviderDisabled(string provider)
        {
            Console.WriteLine(provider + " is disabled ");
        }

        public void OnProviderEnabled(string provider)
        {
            Console.WriteLine(provider + " is enabled ");
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Console.WriteLine(provider + " status has changed " + status.ToString());
        }

        protected override void OnHandleIntent(Intent intent)
        {
            InitializeLoc();
        }

        private string GetProvider()
        {
            var criteria = new Criteria();
            criteria.PowerRequirement = Power.Low;
            criteria.Accuracy = Accuracy.Coarse;

            var provider = locationManager.GetBestProvider(criteria, true);
            return provider;
        }

        private void InitializeLoc()
        {
            locationManager.RequestLocationUpdates(GetProvider(), 2000, 0, this);
        }
    }
}