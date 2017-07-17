using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace SalesApp.Droid.Services.Notification
{
    public abstract class NotificationsHelperBase
    {
        public NotificationsHelperBase(Context context)
        {
            this.Context = context;
            this.AutoCancel = true;
            this.SoundAndOrVibrationAndOrLights = NotificationDefaults.Vibrate | NotificationDefaults.Sound;
        }

        public Context Context { get; set; }

        public abstract string GetTitle();

        public abstract string GetMessage();

        

        public virtual bool AutoCancel { get; set; }

        public virtual NotificationDefaults SoundAndOrVibrationAndOrLights { get; set; }

        public virtual DestinationInformation GetDestinationInformation()
        {
            return null;
        }

        public abstract Task SetOverdueNotificationsAsync(List<string> overdueNotifications);
    }
}