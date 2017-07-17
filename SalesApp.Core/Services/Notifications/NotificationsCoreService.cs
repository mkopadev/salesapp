using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.BL.Models.Notifications;
using SalesApp.Core.Enums.Database;
using SalesApp.Core.Enums.Notification;
using SalesApp.Core.Extensions.Data;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Notifications
{
    public class NotificationsCoreService : SQLiteDataService<SalesAppNotification>
    {
        private ILog _logger;
        private INotificationService _nativeNotificationService;

        private ILog Logger
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

        private INotificationService NativeNotificationService
        {
            get
            {
                if (_nativeNotificationService == null)
                {
                    _nativeNotificationService = Resolver.Instance.Get<INotificationService>();
                }

                return _nativeNotificationService;
            }
        }

        public async Task<bool> IsViewed(BusinessEntityBase model)
        {
            return await IsViewed(model.TableName, model.Id.ToString());
        }

        public async Task<bool> IsViewed(string entity, string entityId)
        {
            SalesAppNotification notification = await GetSingleByCriteria(
                    GetCriteriaBuilder(entity, entityId));
            if (notification == null || notification.Id == default(Guid))
            {
                return false;
            }

            return true;
        }

        public async Task<List<TModel>> GetUnViewedItems<TModel>(List<TModel> itemsToTest)
            where TModel : BusinessEntityBase
        {
            List<TModel> viewedOnes = await GetViewedItems(itemsToTest);
            return itemsToTest.NotIntersecting(viewedOnes);
        }

        public async Task<List<TModel>> GetViewedItems<TModel>(List<TModel> itemsToTest)
            where TModel : BusinessEntityBase
        {
            if (itemsToTest == null || itemsToTest.Count == 0)
            {
                return new List<TModel>();
            }

            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

            List<SalesAppNotification> viewedNotifications = await GetManyByCriteria(
                    criteriaBuilder
                        .Add("Entity", itemsToTest[0].TableName)
                        .Add("EntityId", itemsToTest.Select(item => item.Id).ToArray(), ConjunctionsEnum.And, Operators.In)
                        .Add("NotificationStatus",NotificationStatus.Viewed)
                );
            if (viewedNotifications == null || viewedNotifications.Count == 0)
            {
                return new List<TModel>();
            }

            List<string> intersectingId = viewedNotifications.Select(a => a.EntityId)
                .Intersect(itemsToTest.Select(item => item.Id.ToString())).ToList();

            return itemsToTest.Where(item => intersectingId.Contains(item.Id.ToString())).ToList();
        }

        public async Task<bool> ScheduleNotificationAsync( DateTime dateTime, NotificationTypes notificationType,
            string entity, string entityId)
        {
            try
            {
                Logger.Debug("Attempting to schedule native notification");

                if ((await NativeNotificationService.ScheduleNotificationAsync(
                        dateTime,
                        notificationType,
                        entity,
                        entityId)) == false)
                {
                    throw new Exception("Native exception could not be scheduled");
                }

                Logger.Debug("Native notification service claims to have completed successfully.");

                SalesAppNotification notification = new SalesAppNotification
                {
                    NotificationStatus = NotificationStatus.Scheduled,
                    Entity = entity,
                    EntityId = entityId,
                    NotificationType = notificationType,
                    NotificiationTime = dateTime
                };

                return (await SaveAsync(notification)).SavedModel.Id != default(Guid);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(string entity, string entityId)
        {
            if (await NativeNotificationService.DeleteNotificationAsync(entity, entityId))
            {
                return await this.DeleteWithCriteriaAsync(
                            this.GetCriteriaBuilder(entity, entityId)
                            .Criteria) == 1;
            }

            return false;
        }

        private CriteriaBuilder GetCriteriaBuilder(string entity, string entityId)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            return criteriaBuilder
                .Add("Entity", entity)
                .Add("EntityId", entityId);
        }

        public async Task<bool> UpdateNotificationAsync(string entity, string entityId, DateTime dateTime)
        {
            if (await NativeNotificationService.UpdateNotificationAsync(entity, entityId,dateTime))
            {
                SalesAppNotification notification = await GetNotificationAsync(entity, entityId);
                notification.NotificiationTime = dateTime;
                await SaveAsync(notification);
                return true;
            }

            return false;
        }

        public async Task<SalesAppNotification> GetNotificationAsync(string entity, string entityId)
        {
            SalesAppNotification notification = await this.GetSingleByCriteria(this.GetCriteriaBuilder(entity, entityId));
            if (notification == null || notification.Id == default(Guid))
            {
                string message = string.Format("Cannot find notification for entity '{0}' with id '{1}' in the database", entity, entityId);
                throw new Exception(message);
            }

            return notification;
        }

        public async Task<SaveResponse<SalesAppNotification>> SetSingleNotificationViewed(string entity, string entityId)
        {
            SalesAppNotification notification = await GetNotificationAsync(entity, entityId);
            if (notification == null || notification.NotificationStatus == NotificationStatus.Viewed)
            {
                return new SaveResponse<SalesAppNotification>(notification ?? new SalesAppNotification(), new PostResponse());
            }
            return await SetSingleNotificationViewed(notification);
        }

        public async Task<List<string>> GetOverdueNotificationsEntityIdsAsync(NotificationTypes notificationType = default(NotificationTypes))
        {
            return (await GetAllOverdueNotificationsAsync(notificationType))
                .Select(notification => notification.EntityId.ToString())
                .ToList();
        }

        

        public async Task<SaveResponse<SalesAppNotification>>  SetSingleNotificationViewed(SalesAppNotification notification)
        {
            notification.NotificationStatus = NotificationStatus.Viewed;
            return await SaveAsync(notification);
        }

        public async Task SetAllOverdueNotificationsViewed(NotificationTypes notificationTypes)
        {
            List<SalesAppNotification> overdueNotifications = await GetAllOverdueNotificationsAsync(notificationTypes);
            foreach (var overdueNotification in overdueNotifications)
            {
                await SetSingleNotificationViewed(overdueNotification);
            }
        }

        public async Task<List<SalesAppNotification>> GetAllOverdueNotificationsAsync(NotificationTypes notificationType = default (NotificationTypes))
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

            List<SalesAppNotification> overdueNotifications = await GetManyByCriteria
                (
                    criteriaBuilder
                        .AddDateCriterion("NotificiationTime", DateTime.Now, ConjunctionsEnum.And,
                            Operators.LessThanOrEqualTo, true, true)
                        .AddIfTrue(notificationType != default(NotificationTypes), "NotificationType", (int)notificationType)
                        .Add("NotificationStatus", (int)NotificationStatus.Viewed, ConjunctionsEnum.And, Operators.NotEqualTo)
                ) ?? new List<SalesAppNotification>();
            return overdueNotifications;
        }

        

        
    }
}