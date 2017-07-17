using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.Tests.Services.Database
{
    [TestFixture]
    public class SQLiteDataServiceTests : TestsBase
    {
        private ProspectsController prospectsController
        {
            get
            {
                if (_prospectsController == null)
                {
                    _prospectsController = new ProspectsController(LanguagesEnum.EN,
                        CountryCodes.KE);
                }

                return _prospectsController;
            }
        }

        private ProspectsController _prospectsController;

        private async Task<SaveResponse<Prospect>> SaveAsync(string firstname, string lastname,string phone)
        {
            Prospect prospect = new Prospect
            {
                LastName = lastname
                ,
                FirstName = firstname
                ,
                Authority = true
                ,
                DsrPhone = "0721554712"
                ,
                Money = true
                ,
                Need = false
                ,
                Phone = phone
            };

            return await prospectsController.SaveAsync(prospect);
        }

        private async Task<SaveResponse<Prospect>> SaveAsync()
        {
            return await SaveAsync("Tony", "Stark", "0722553229");
        }

        [SetUp]
        public void Initialize()
        {
            Console.WriteLine("Setting up");
            try
            {
                //new DatabaseVersioning().ManageDbVersions(sqlDB);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async Task SaveAsyncTest()
        {

            SaveResponse<Prospect> prospectResponse = await SaveAsync();
            Assert.AreNotEqual(prospectResponse.SavedModel.Id, default(Guid), "A saved model's ID should be not be the default value for guids");
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async void GetAllAsyncTest()
        {
            SaveResponse<Prospect> prospectResponse = await SaveAsync();
            List<Prospect> prospects = await prospectsController.GetAllAsync();
            foreach (var p in prospects)
            {
                Debug.WriteLine(string.Format("{2}: {0} {1} {3}", p.FirstName, p.LastName, p.Id, p.Created.ToLongDateString()));
            }
           Assert.Greater(prospects.Count,0,"Looks like either saving or fetching is failing");
           await Delete(prospectResponse.SavedModel);
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async void GetByIdAsyncTest()
        {
            SaveResponse<Prospect> prospectResponse = await SaveAsync();
            Prospect prospectCopy = await prospectsController.GetByIdAsync(prospectResponse.SavedModel.Id);
            Debug.WriteLine(prospectResponse.SavedModel.FirstName);
            Assert.IsNotNull(prospectResponse.SavedModel, "Either saving or retrieving does not work.");
            await Delete(prospectResponse.SavedModel);
        }

        [Test]
        public async void GetByNonExistantIdAsyncTest()
        {
            Prospect prospect = await prospectsController.GetByIdAsync(Guid.NewGuid());
            Assert.IsNull(prospect, "Prospect should always be null");
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async void DeleteByIdAsyncTest()
        {
            SaveResponse<Prospect> prospectResponse = await SaveAsync();
            int result = await prospectsController.DeleteAsync(prospectResponse.SavedModel.Id);
            Assert.IsTrue(result == 1,"Could not delete by id.");
        }

        [Test]
        public async void DeleteNonExistantRecord()
        {
           int result = await prospectsController.DeleteAsync(Guid.NewGuid());
           Assert.IsFalse(result == 1, "Appears as though a non existant record was deleted");
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async void DeleteModelAsyncTest()
        {
            SaveResponse<Prospect> prospectResponse = await SaveAsync();
            int result = await prospectsController.DeleteAsync(prospectResponse.SavedModel);
            Assert.IsTrue(result == 1, "Could not delete model");
        }

        private async Task Delete(Prospect prospect)
        {
            await prospectsController.DeleteAsync(prospect);
        }

        // [Test] Todo ILocationServiceListener not bound to a real implementation, cannot be resolved
        public async void SelectQueryTest()
        {
            const string firstname = "Mr";
            const string lastname = "Waweru";

            List<Prospect> results = await prospectsController.SelectQueryAsync(
                new[]
                    {
                        new Criterion("FirstName", firstname),
                        new Criterion("LastName",lastname)
                    });

            foreach (var prosp in results)
            {
                await prospectsController.DeleteAsync(prosp);
            }

            SaveResponse<Prospect> prospectResponse = await SaveAsync(firstname, lastname, "0724569852");

            results = await prospectsController.SelectQueryAsync(
                new []
                {
                    new Criterion("FirstName", firstname),
                    new Criterion("LastName",lastname)
                });

            Assert.AreEqual(prospectResponse.SavedModel.Id, results[0].Id);
        }
    }
}
