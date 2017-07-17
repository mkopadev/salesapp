using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mkopa.Core;
using Mkopa.Core.Api.Security;
using Mkopa.Core.BL.Controllers.Security;
using Mkopa.Core.BL.Models;
using Mkopa.Core.BL.Models.Security;
using Mkopa.Core.Enums.MultiCountry;
using Mkopa.Core.Services.Database;
using Mkopa.Core.Services.DependancyInjection;
using NUnit.Framework;
using SQLite.Net.Platform.XamarinAndroid;

namespace MKopa.CoreTests.Api.Security
{
    [TestFixture]
    public class LoginApisTest
    {
        private ISQLiteDB _sqlDB = null;
        private ISQLiteDB sqlDB
        {
            get
            {
                if (_sqlDB == null)
                {
                    _sqlDB = Resolver.Instance.Get<ISQLiteDB>();
                }
                return _sqlDB;
            }
        }
        [SetUp]
        public void Initialize()
        {
            new TestBootstrapper().Bootstrap();
        }

        [Test]
        public async Task LoginTest()
        {
            LoginApis loginApis = new LoginApis
                (
                    LanguagesEnum.EN
                    ,CountryCodes.KE
                    ,sqlDB
                );
            LoginResponse loginResponse = await loginApis.Login("0721553229", "1234", true);
            List<Permission> existingPermissions = await PermissionsController.Instance.GetAllAsync();
            var downloadedPermissions = loginResponse.Permissions.Select(p => new {p.PermissionId })
                .ToList();
            List<Permission> matchingPermissions = existingPermissions
                .Where
                (
                    perm => downloadedPermissions.Contains(new { perm.PermissionId })
                ).ToList();

            Assert.AreEqual(downloadedPermissions.Count,matchingPermissions.Count);
        }
    }
}