using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Database;
using SalesApp.Core.Extensions.Data;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;
using Enumerable = System.Linq.Enumerable;

namespace SalesApp.Droid.Services.Notification.Helpers
{
    public class ProspectsReminderNotificationsHelper : NotificationsHelperBase
    {
        private ProspectFollowUpsController _followUpsController = new ProspectFollowUpsController();
        private ProspectsController _prospectsController = new ProspectsController();
        private List<ProspectFollowup> _followUps;
        private List<string> _overdueNotifications;
        private Prospect _singleProspect;


        public override string GetTitle()
        {
            if (_followUps.Count == 1)
            {
                return Context.GetString(Resource.String.prospect_reminder_notification_title_single);
            }

            return Context.GetString(Resource.String.prospect_reminder_notification_title_multiple);
        }

        public override string GetMessage()
        {
            if (_followUps.Count == 1)
            {
                
                return string.Format
                    (
                        Context.GetString(Resource.String.prospect_reminder_notification_message_single)
                        , _singleProspect.FullName
                    );
            }

            return string.Format
                (
                    Context.GetString(Resource.String.prospect_reminder_notification_message_multiple)
                    , _followUps.Count
                );
        }

        public override NotificationDefaults SoundAndOrVibrationAndOrLights
        {
            get { return NotificationDefaults.Sound | NotificationDefaults.Vibrate; }
        }

        public ProspectsReminderNotificationsHelper(Context context) : base(context)
        {
        }

        public override DestinationInformation GetDestinationInformation()
        {
            if (_followUps.Count == 0)
            {
                return null;
            }

            if (_followUps.Count == 1)
            {
                Intent intent = new ProspectDetailsHelper().GetPropsectDetailIntent
                (
                     Context,
                     new ProspectItem(_singleProspect.CastTo<ProspectSearchResult>())
                );

                intent.PutExtra(ProspectDetailActivity.ProspectDetailsOrigin, ProspectDetailsOrigin.ProspectReminderClick.ToString());

                return new DestinationInformation
                {
                    ContentIntent = intent,
                    ActivityType = typeof(ProspectListView)
                };
            }

            return new DestinationInformation
            {
                ContentIntent = new Intent(Context, typeof (ProspectListView)),
                ActivityType = typeof (ProspectListView)
            };
        }

        public override async Task SetOverdueNotificationsAsync(List<string> overdueNotifications)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            _overdueNotifications = overdueNotifications;
            _followUps = await _followUpsController.GetManyByCriteria
                (
                    criteriaBuilder
                        .Add("Id", _overdueNotifications.ToArray(), ConjunctionsEnum.And, Operators.In)
                );
            List<Guid> followUpsToRemove = new List<Guid>();
            for (int i = 0; i < _followUps.Count; i++)
            {
                
                if ((await ProspectWasConverted(_followUps[i])))
                {
                    followUpsToRemove.Add(_followUps[i].Id);
                }
            }
            if (followUpsToRemove.Count > 0)
            {
                _followUps = Enumerable.ToList(Enumerable.Where(_followUps, followup => !followUpsToRemove.Contains(followup.Id)));
            }

            if (_followUps.Count == 1 && _singleProspect == null)
            {
                _singleProspect = await new ProspectsController().GetByIdAsync(_followUps[0].ProspectId);
                _singleProspect.ReminderTime = _followUps[0].ReminderTime;
            }
        }
        
        private async Task<bool> ProspectWasConverted(ProspectFollowup followup)
        {
            Prospect prospect = await _prospectsController.GetByIdAsync(followup.ProspectId);
            if (prospect == null)
            {
                return true;
            }

            return prospect.Converted;
        }
    }
}