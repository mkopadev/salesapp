using System.Collections.Generic;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Framework;

namespace SalesApp.Core.Services
{
    public class ConfigService : IConfigService
    {
        private readonly Settings.Settings _settings = Settings.Settings.Instance;

        // api urls
        private const string ApiReleaseKenya = @"http://ke-release-api-url/";
        private const string ApiReleaseTanzania = @"http://tz-release-api-url/";
        private const string ApiReleaseUganda = @"http://ug-release-api-url/";
        private const string ApiReleaseGhana = @"http://gh-release-api-url/";

        private const string ApiTestKenya = @"http://private-f0ec5-mkopasalesappapi.apiary-mock.com/api/";
        private const string ApiTestTanzania = @"http://ke-test-api-url/";
        private const string ApiTestUganda = @"http://ug-test-api-url/";
        private const string ApiTestGhana = @"http://gh-test-api-url/";

        private const string ApiStagingKenya = @"http://ke-staging-api-url/";
        private const string ApiStagingTanzania = @"http://tz-staging-api-url/";
        private const string ApiStagingUganda = @"http://ug-staging-api-url/";
        private const string ApiStagingGhana = @"http://gh-staging-api-url/";

        // define API dictionaries
        private readonly Dictionary<CountryCodes, string> _apiRelease = new Dictionary<CountryCodes, string>
        {
            { CountryCodes.KE, ApiReleaseKenya },
            { CountryCodes.TZ, ApiReleaseTanzania },
            { CountryCodes.UG, ApiReleaseUganda },
            { CountryCodes.GH, ApiReleaseGhana }
        };

        private readonly Dictionary<CountryCodes, string> _apiTest = new Dictionary<CountryCodes, string>
        {
            { CountryCodes.KE, ApiTestKenya },
            { CountryCodes.TZ, ApiTestTanzania },
            { CountryCodes.UG, ApiTestUganda },
            { CountryCodes.GH, ApiTestGhana }
        };

        private readonly Dictionary<CountryCodes, string> _apiStaging = new Dictionary<CountryCodes, string>
        {
            { CountryCodes.KE, ApiStagingKenya },
            { CountryCodes.TZ, ApiStagingTanzania },
            { CountryCodes.UG, ApiStagingUganda },
            { CountryCodes.GH, ApiStagingGhana }
        };

        [Preserve]
        public ConfigService()
        {
        }

        // config files not present in Xamarin, using hardcoded config for now
        public string ApiUrl
        {
            get
            {
                #if DEBUG || UAT
                    return this._apiTest[this._settings.DsrCountryCode];
                #elif STAGING
                    return this._apiStaging[this._settings.DsrCountryCode];
                #else
                    return this._apiRelease[this._settings.DsrCountryCode];
                #endif
            }
        }
    }
}
