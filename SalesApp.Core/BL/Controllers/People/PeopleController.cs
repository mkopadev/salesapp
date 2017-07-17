using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.Api.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.Database;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Validation;

namespace SalesApp.Core.BL.Controllers.People
{
    public abstract class PeopleController<T> : SQLiteDataService<T> where T : Person, new()
    {
        private Settings legacySettings = Settings.Instance;

        protected PeopleController() : base()
        {
        }

        /// <summary>
        /// Returns the customer or prospect object that matches the specified phone number
        /// </summary>
        /// <param name="phone">The phone number to search by</param>
        /// <returns>Returns the customer or prospect object</returns>
        /// <exception cref="DuplicateValuesException">Throws an exception of more than one record with the specified phone number is found</exception>
        public async Task<T> GetByPhoneNumberAsync(string phone, bool checkDuplicate = true)
        {
            List<T> people = await this.SelectQueryAsync(
                    new[]
                    {
                        new Criterion("Phone", phone)
                    });

            if (people == default(List<T>))
            {
                return default(T);
            }

            if (checkDuplicate)
            {
                if (people.Count >= 1)
                {
                    throw new DuplicateValuesException(typeof(T).FullName, phone, people.Count);
                }
            }

            if (people.Count == 0)
            {
                return default(T);
            }

            return people[0];
        }

        public async Task<int> DeleteByPhoneNumberAsync(string phone)
        {
            return await this.DeleteWithCriteriaAsync(
                new[]
                {
                    new Criterion("Phone", phone)
                });
        }

        public bool ValidateBasicInfo(T person)
        {
            PeopleDetailsValidater validator = new PeopleDetailsValidater();
            return validator.PersonDetailsValid(person, this.legacySettings.DsrCountryCode);
        }

        public async Task<T> SearchPersonOnlineAsync(string phone, string nationalId = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            string urlParams = string.Format("?phoneNumber={0}", phone);

            if (!string.IsNullOrEmpty(nationalId))
            {
                urlParams += string.Format("&idNumber={0}", nationalId);
            }

            T person = await new PeopleApis<T>("persons/search").Search(urlParams, filterFlags, timeOut);
            return person;
        }

        public async Task<T> GetPersonIfExists(string phone, string nationalId = null, ErrorFilterFlags filterFlags = ErrorFilterFlags.EnableErrorHandling, bool checkDuplicate = false, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            T person = await this.GetByPhoneNumberAsync(phone, checkDuplicate);
            if (person != null)
            {
                return person;
            }

            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                return null;
            }

            person = await this.SearchPersonOnlineAsync(phone, nationalId, filterFlags, timeOut);

            return person;
        }
    }
}
