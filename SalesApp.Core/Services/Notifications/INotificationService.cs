using System;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Notifications;
using SalesApp.Core.Enums.Notification;

namespace SalesApp.Core.Services.Notifications
{
    public interface INotificationService
    {
        /// <summary>
        /// Creates a notification
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="notificationType"></param>
        /// <param name="entity"></param>
        /// <param name="entityId"></param>
        /// <returns>Information about the saved notification</returns>
        Task<bool> ScheduleNotificationAsync(DateTime dateTime, NotificationTypes notificationType, string entity, string entityId);

        /// <summary>
        /// Deletes the notification identified by specified entity and entityId
        /// </summary>
        Task<bool> DeleteNotificationAsync(string entity, string entityId);

        /// <summary>
        /// Updates the notification identified by specified entity and entityId
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityId"></param>
        /// <param name="dateTime"></param>
        Task<bool> UpdateNotificationAsync(string entity, string entityId, DateTime dateTime);

        /// <summary>
        /// Returns the notification that matches the specified entity and entityId
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task<SalesAppNotification> GetNotificationAsync(string entity, string entityId);

    }
}