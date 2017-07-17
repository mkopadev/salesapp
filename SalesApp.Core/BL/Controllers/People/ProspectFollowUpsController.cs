using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Database;
using SalesApp.Core.Enums.Notification;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Exceptions.Validation.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.Notifications;

namespace SalesApp.Core.BL.Controllers.People
{
    public class ProspectFollowUpsController : SQLiteDataService<ProspectFollowup>
    {
        public async Task<SaveResponse<ProspectFollowup>> SaveFollowupAsync(ProspectFollowup model)
        {
            try
            {
                Logger
                    .Debug("Prospect date is ~".GetFormated(model.ReminderTime));
                Logger.Debug("Validating log time");
                if (model.ReminderTime < DateTime.Now)
                {
                    if (model.ReminderTime.Date < DateTime.Today)
                    {
                        throw new ProspectFollowUpInvalidException(ProspectFollowUpValidationResultsEnum.PastDate);
                    }
                    else
                    {
                        throw new ProspectFollowUpInvalidException(ProspectFollowUpValidationResultsEnum.PastTime);
                    }
                }

                Logger.Debug("Saving followup");

                var result = await OverwriteAsync(model, "ProspectId");
                Logger.Debug("Id of saved reminder is ~".GetFormated(result.SavedModel.Id));
                if (result.SavedModel.Id == default(Guid))
                {
                    Logger.Debug("Could not save prospect reminder");
                    return result;
                }

                Logger.Debug("Writing notification record to db");
                await new NotificationsCoreService().ScheduleNotificationAsync
                    (
                        result.SavedModel.ReminderTime
                        , NotificationTypes.ProspectReminder
                        , result.SavedModel.TableName
                        , result.SavedModel.Id.ToString()
                    );
                Logger.Debug("Notification record written");

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

        }

        public async Task<DateTime> GetReminderDate(Guid prospectId)
        {
            List<ProspectFollowup> reminders = await SelectQueryAsync(new Criterion[]
                    {
                        new Criterion("ProspectId", prospectId)
                    });

            if (reminders != null && reminders.Count > 0)
            {
                return reminders.OrderByDescending(reminder => reminder.Modified).First().ReminderTime;
            }

            return default(DateTime);
        }

        public async Task<DateTime> GetReminderDateSync(Guid prospectId)
        {
            List<ProspectFollowup> reminders = await SelectQueryAsync(
                new Criterion[]
                    {
                        new Criterion("ProspectId", prospectId)
                    });

            if (reminders != null && reminders.Count > 0)
            {
                return reminders.OrderByDescending(reminder => reminder.Modified).First().ReminderTime;
            }

            return default(DateTime);
        }

        public async Task<List<ProspectFollowup>> GetOverdueRemindersAsync()
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            List<ProspectFollowup> followups = await GetManyByCriteria(
                    criteriaBuilder
                        .AddDateCriterion("ReminderTime", DateTime.Now, ConjunctionsEnum.And,
                            Operators.LessThanOrEqualTo, true, true));

            if (followups == null || followups.Count == 0)
            {
                Logger.Debug("There are no followup reminders");
                return new List<ProspectFollowup>();
            }

            Logger.Debug("Number of over due reminders is ~".GetFormated(followups.Count));
            string[] prospectIds = followups.Where(followup => followup.ProspectId != default(Guid))
                .Select(a => a.ProspectId.ToString())
                .ToArray();

            Logger.Debug("No null prospect ids count is ~".GetFormated(prospectIds.Length));

            criteriaBuilder = new CriteriaBuilder();

            List<Prospect> prospects = await new ProspectsController().GetManyByCriteria(criteriaBuilder
                        .Add("Id", prospectIds, ConjunctionsEnum.And, Operators.In));

            if (prospects == null || prospects.Count == 0)
            {
                return new List<ProspectFollowup>();
            }

            return
                followups.Where(followup => prospects.Select(prosp => prosp.Id).Contains(followup.ProspectId)).ToList();
        }
    }
}
