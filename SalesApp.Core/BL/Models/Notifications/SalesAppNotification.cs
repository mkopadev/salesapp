using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Notification;

namespace SalesApp.Core.BL.Models.Notifications
{
    public class SalesAppNotification : BusinessEntityBase
    {
        public string Entity { get; set; }
        public string EntityId { get; set; }

        public string NotificationTag { get; set; }

        public NotificationStatus NotificationStatus { get; set; }

        public NotificationTypes NotificationType { get; set; }

        public DateTime NotificiationTime { get; set; }

        
        
    }
}