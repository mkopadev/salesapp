using System;
using NUnit.Framework;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.Tests.BL.Controllers.People
{
    [TestFixture]
    public class ProspectFollowUpsControllerTests
    {
        private ProspectsController _prospectsController;

        private ProspectsController prospectsController
        {
            get
            {
                if (_prospectsController == null)
                {
                    _prospectsController = new ProspectsController ();
                }
                return _prospectsController;
            }
        }

        private ProspectFollowUpsController FollowUpsController
        {
            get
            {
                if (_followUpsController == null)
                {
                    _followUpsController = new ProspectFollowUpsController();
                }
                return _followUpsController;
            }
        }

        private ProspectFollowUpsController _followUpsController;

        [Test]
        public async void GetOverDueReminders()
        {
        }
        
        // [Test] Todo Test completely broken ILog not bound to real implementatio, cannot be resolved
        public async void SaveFollowupAsyncTest()
        {
            const string phone = "0727650004";
            Prospect prospect = new Prospect
            {
                DsrPhone = phone
                ,
                LastName = "User"
                ,
                FirstName = "Test"
                ,
                Phone = phone
                ,
                Authority = true
                ,
                Need = false
                ,
                Money = true
               

            };
            SaveResponse<Prospect> prospectResponse = await prospectsController.SaveAsync(prospect);
            if (prospectResponse.SavedModel.Id == default(Guid))
            {
                throw new Exception("Could not save prospect");
            }


            ProspectFollowup followUp = new ProspectFollowup
            {
                ReminderTime = DateTime.Now.AddSeconds(30)
                ,
                ProspectId = prospect.Id

            };

            
            
            await FollowUpsController.SaveAsync
                (
                    followUp
                );
            
            await FollowUpsController.DeleteAsync(followUp);
            await prospectsController.DeleteAsync(prospect);
            Assert.AreNotEqual(followUp.Id, default(Guid));

        }
    }
}