using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Exceptions.Database;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.Auth
{
    public class LoginController : SQLiteDataService<DsrProfile>
    {
        protected LanguagesEnum Lang { get; set; }

        protected CountryCodes Country { get; set; }

        public LoginController(LanguagesEnum lang, CountryCodes country)
        {
            this.Country = country;
            this.Lang = lang;
        }

        public async Task<DsrProfile> GetByDsrPhoneNumberAsync(string dsrPhone)
        {
            List<DsrProfile> profile = await SelectQueryAsync(
                    new Criterion[]
                    {
                        new Criterion("DsrPhone", dsrPhone)
                    });

            if (profile == default(List<DsrProfile>))
            {
                return default(DsrProfile);
            }

            if (profile.Count > 1)
            {
                throw new DuplicateValuesException();
            }

            if (profile.Count == 0)
            {
                return null;
            }

            return profile[0];
        }
    }
}
