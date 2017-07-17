using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Tests.BL.Controllers.People
{
    public class ProspectsControllerTests : TestsBase
    {
        [Test]
        public void GetAllPropsects()
        {
            Assert.DoesNotThrow
                (
                    async () =>
                    {
                        List<Prospect> prospects = await Resolver.Instance.Get<ProspectsController>()
                            .GetAllAsync();
                        for (int i = 0; i < prospects.Count; i++)
                        {
                            Logger.Debug("~:). ~ ".GetFormated(i, prospects[i].FullName));
                        }
                    }
                );
        }

        private async Task<int> DeleteProspectIfExists(string phone)
        {
            Prospect prospect = await Resolver.Instance.Get<ProspectsController>().GetByPhoneNumberAsync(phone);
            int result = 0;
            if (prospect != null)
            {
                result = await Resolver.Instance.Get<ProspectsController>().DeleteAsync(prospect);
                if (result != 1)
                {
                    throw new Exception("Could not delete prospect");
                }

            }
            return result;
        }

        // [Test] Todo test throws exception Cannot perform this test when a connection is avaliable
        public async Task SaveProspectOffline()
        {
            if (Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                throw new Exception("Cannot perform this test when a connection is avaliable");
            }

            const string phone = "0784159756";
            int saveResult = await DeleteProspectIfExists(phone);
            "results of deletion were ~".WriteLine(saveResult.ToString());
            if (saveResult == 1 || saveResult == 0)
            {
                "we deleted successfully".WriteLine();
                SaveResponse<Prospect> prospectResponse = await Resolver.Instance.Get<ProspectsController>().SaveAsync
                    (
                        new Prospect
                        {
                            Authority = true
                            ,
                            DsrPhone = phone
                            ,
                            LastName = "Mathenge"
                            ,
                            FirstName = "Thomas"
                            ,
                            Money = true
                            ,
                            Need = true
                            
                            ,
                            Phone = phone
                        }
                    );
                Logger.Debug("Already out of save async");
                Assert.AreNotSame(prospectResponse.SavedModel.Id,default(Guid));
                SyncRecord syncRecord = await Resolver.Instance.GetInstance<SyncingController>()
                    .GetSyncRecordAsync(prospectResponse.SavedModel.RequestId);
                Assert.True(syncRecord.Status == RecordStatus.Pending,"Status should not be equal to " + syncRecord.Status.ToString());
                
            }
            else
            {
                throw new Exception("Could not delete prospect");
            }
            
        }
    }
}