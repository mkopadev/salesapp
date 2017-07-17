using Android.Content;
using Android.Net;
using SalesApp.Core.Services.Connectivity;

namespace SalesApp.Droid.Services.Connectivity
{
    public class ConnectivityService : IConnectivityService
    {
        public bool HasConnection()
        {   
            ConnectivityManager connectivityManager = 
                SalesApplication
                .Instance
                .GetSystemService(Context.ConnectivityService) as ConnectivityManager;

            if (connectivityManager == null)
            {
                return false;
            }

            return connectivityManager.ActiveNetworkInfo != null && connectivityManager.ActiveNetworkInfo.IsConnected;
        }
    }
}