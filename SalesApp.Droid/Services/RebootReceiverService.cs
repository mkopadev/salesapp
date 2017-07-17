using Android.App;
using Android.Content;
using SalesApp.Droid.Services.Notification;

namespace SalesApp.Droid.Services
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class RebootReceiverService : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            new LocalNotificationService().ShowOverdueNotifications();
        }
    }
}