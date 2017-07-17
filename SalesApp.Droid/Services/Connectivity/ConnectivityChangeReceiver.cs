using System;
using Android.Content;

namespace SalesApp.Droid.Services.Connectivity
{
    [BroadcastReceiver]
    public class ConnectivityChangeReceiver : BroadcastReceiver
    {
        public EventHandler ConnectionChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            if (this.ConnectionChanged != null)
            {
                this.ConnectionChanged(this, EventArgs.Empty);
            }
        }
    }
}