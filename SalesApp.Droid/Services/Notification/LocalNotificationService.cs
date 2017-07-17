using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Java.Lang;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Notifications;
using SalesApp.Core.Enums.Notification;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Notifications;
using SalesApp.Core.Services.Security;
using SalesApp.Droid.Services.Notification.Helpers;
using Exception = System.Exception;
using TaskStackBuilder = Android.App.TaskStackBuilder;

namespace SalesApp.Droid.Services.Notification
{
    [BroadcastReceiver]
    public class LocalNotificationService : BroadcastReceiver , INotificationService
    {

        private const string NotificationType = "NotificationType";
        private const string WakelockTag = "NotificationsWakeLock";

        private ILog _logger;
        

        
        /// <summary>
        /// Looks like you're trying to instantiate this class. Unless you really have a good reason, use the notification service in core as it ties into functionality to track notifications in the database : )
        /// </summary>
        [Obsolete]
        public LocalNotificationService()
        {

        }

        

        private Context AppContext
        {
            get { return SalesApplication.Instance.ApplicationContext; }
        }

        public ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }
                return _logger;
            }
        }


        

        private long GetTriggerTime(DateTime dateTime)
        {
            return (long) (dateTime.ToMilliSeconds());

        }

        public void DeleteNotification(int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateNotification(int id)
        {
            throw new NotImplementedException();
        }

        private NotificationsHelperBase GetHelper(NotificationTypes notificationType)
        {
            switch (notificationType)
            {
                    case NotificationTypes.ProspectReminder:
                        return new ProspectsReminderNotificationsHelper(AppContext);
            }
            throw new Exception("Notification helper for notification type '" + notificationType.ToString() + "' has not been implemented");
        }

        private async Task<Intent> GetContentIntentAsync(NotificationsHelperBase notificationHelper, TaskStackBuilder taskStackBuilder)
        {
            bool loginExpired = !(await new LoginService().GetLoginValidityAsync());
            
            ISalesAppSession session = Resolver.Instance.Get<ISalesAppSession>();
            if (loginExpired || session == null || session.UserId == default(Guid))
            {
                return new Intent(AppContext,typeof(LoginActivityView));
            }
            else
            {
                Logger.Debug("Session id is " + session.UserId);
                DestinationInformation destinationInformation = notificationHelper.GetDestinationInformation();
                taskStackBuilder.AddParentStack(Class.FromType(destinationInformation.ActivityType));
                taskStackBuilder.AddNextIntent(destinationInformation.ContentIntent);
                return destinationInformation.ContentIntent;
            }
        }


        public async Task ShowOverdueNotifications()
        {
            List<SalesAppNotification> notifications = await new NotificationsCoreService()
                .GetAllOverdueNotificationsAsync();
            foreach (var notification in notifications)
            {
                await ScheduleNotificationAsync
                    (
                        notification.NotificiationTime
                        , notification.NotificationType
                        , notification.Entity
                        , notification.EntityId
                    );
            }
        }


        public override void OnReceive(Context context, Intent intent)
        {
            Task.Run
                (
                    async () =>
                    {
                        try
                        {
                            // Wakelocker.Acquire(AppContext, WakelockTag);
                            // await new LocalOtaService().CacheAsync();
                            Logger.Debug("Notification OnReceive called");
                            int savedNotificationType = intent.GetIntExtra(NotificationType, 0);
                            if (savedNotificationType == 0)
                            {
                                Logger.Error("Unknown notification type");
                                return;
                            }
                            Logger.Debug("Notification type is " + savedNotificationType);
                            NotificationTypes notificationType = (NotificationTypes) savedNotificationType;
                            Logger.Debug("Showing notification of type " + notificationType.ToString());


                            if (notificationType == default(NotificationTypes))
                            {
                                return;
                            }



                            Logger.Debug("Booting up the core notification service");
                            NotificationsCoreService notificationsCoreService = new NotificationsCoreService();
                            Logger.Debug("Requesting core notification service for overdue notifications");
                            List<string> overdueNotifications =
                                await notificationsCoreService.GetOverdueNotificationsEntityIdsAsync(notificationType);

                            Logger.Debug("Establishing the notification helper to use");
                            NotificationsHelperBase notificationHelper = GetHelper(notificationType);

                            Logger.Debug("Asking notification helper to filter out notifications that aren't needed");
                            await notificationHelper.SetOverdueNotificationsAsync(overdueNotifications);

                            if (overdueNotifications == null)
                            {
                                overdueNotifications = new List<string>();
                            }
                            if (overdueNotifications.Count == 0)
                            {
                                Logger.Debug("Turns out there are no notifications to show");

                                return;
                            }


                            string message = notificationHelper.GetMessage();
                            PendingIntent contentIntent = PendingIntent.GetActivity(context, default(int), intent,
                                PendingIntentFlags.CancelCurrent);



                            var notificationManager = NotificationManagerCompat.From(context);
                            var style = new NotificationCompat.BigTextStyle();
                            style.BigText(message);


                            var builder = new NotificationCompat.Builder(AppContext)
                                .SetContentTitle(notificationHelper.GetTitle())
                                .SetContentText(notificationHelper.GetMessage())
                                .SetAutoCancel(notificationHelper.AutoCancel)
                                .SetContentIntent(contentIntent)
                                .SetSmallIcon(Resource.Drawable.Icon22x22)
                                .SetStyle(style)
                                .SetDefaults((int) notificationHelper.SoundAndOrVibrationAndOrLights)
                                .SetWhen(JavaSystem.CurrentTimeMillis());

                            TaskStackBuilder taskStackBuilder = TaskStackBuilder.Create(AppContext);
                            Intent destIntent = await GetContentIntentAsync(notificationHelper, taskStackBuilder);

                            if (destIntent != null)
                            {
                                PendingIntent pendingIntent = taskStackBuilder.GetPendingIntent(0,
                                    PendingIntentFlags.OneShot);
                                    //PendingIntent.GetActivity(AppContext, 0, destIntent, PendingIntentFlags.OneShot);
                                builder.SetContentIntent(pendingIntent);
                            }
                            var notification = builder.Build();
                            notificationManager.Notify((int) notificationType, notification);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            throw;
                        }
                        finally
                        {
                            Wakelocker.Release();
                        }

                    }
                );
        }

        

        public async Task<bool> ScheduleNotificationAsync(DateTime dateTime, NotificationTypes notificationType, string entity, string entityId)
        {
            AlarmManager alarmManager = AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                return false;
            }
            Intent intent = new Intent(AppContext, this.Class);
            intent.PutExtra(NotificationType, (int)notificationType);
            int entityIdHashCode = entityId.GetHashCode();
            Logger.Debug("Hash code for the entity id is " + entityIdHashCode);
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(AppContext,entityIdHashCode, intent, 0);

            long triggerTime = GetTriggerTime(dateTime);
            alarmManager.Set
                (
                    AlarmType.RtcWakeup, triggerTime, pendingIntent
                );
            return true;
        }

        public Task<bool> DeleteNotificationAsync(string entity, string entityId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateNotificationAsync(string entity, string entityId, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<SalesAppNotification> GetNotificationAsync(string entity, string entityId)
        {
            throw new NotImplementedException();
        }
    }
}