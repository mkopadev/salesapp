using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Exceptions.Validation.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Core.Validation
{
    /// <summary>
    /// This class contains commonly used validation calls
    /// </summary>
    public class PeopleDetailsValidater : IDisposable
    {
        private const int NameMaxLength = 255;
        public const int NationalIdLength = 7;

        /// <summary>
        /// Validates a phone number and returns true on success or false on failure.
        /// </summary>
        /// <param name="phoneNumber">The phone number to be validated</param>
        /// <returns>True if the number is successful or false on failure.</returns>
        public PhoneValidationResultEnum ValidatePhoneNumber(string phoneNumber)
        {
            CountryCodes countryCode = Settings.Instance.DsrCountryCode;

            if (phoneNumber.IsBlank())
            {
                return PhoneValidationResultEnum.NullEntry;
            }

            if (!new Regex(@"^[0-9]+$").IsMatch(phoneNumber))
            {
                return PhoneValidationResultEnum.InvalidCharacters;
            }

            if (phoneNumber.Length > 10)
            {
                return PhoneValidationResultEnum.NumberTooLong;
            }

            if (phoneNumber.Length < 10)
            {
                return PhoneValidationResultEnum.NumberTooShort;
            }

            // Look for phone number validation
            var countryRegex =
                new Dictionary<CountryCodes, string>
                {
                    { CountryCodes.KE, @"^(07)\+?[\d\s]{8}$" },
                    { CountryCodes.UG, @"^(03|07)\+?[\d\s]{8}$" },
                    { CountryCodes.GH, @"^(02|05)\+?[\d\s]{8}$" },
                    { CountryCodes.TZ, @"^(07)\+?[\d\s]{8}$" }
                };

            string pattern = countryRegex[countryCode];

            if (!new Regex(pattern).IsMatch(phoneNumber))
            {
                return PhoneValidationResultEnum.InvalidFormat;
            }

            return PhoneValidationResultEnum.NumberOk;
        }

        public bool PersonDetailsValid(Person person,CountryCodes country)
        {
            switch (this.ValidateName(person.FirstName))
            {
                case PersonNameValidationResultsEnum.NameBlank:
                    throw new PersonNameInvalidException(PersonNameValidationResultsEnum.FirstNameBlank);
                case PersonNameValidationResultsEnum.NameTooLong:
                    throw new PersonNameInvalidException(PersonNameValidationResultsEnum.FirstNameTooLong);
            }

            switch (this.ValidateName(person.LastName))
            {
                case PersonNameValidationResultsEnum.NameBlank:
                    throw new PersonNameInvalidException(PersonNameValidationResultsEnum.LastNameBlank);
                case PersonNameValidationResultsEnum.NameTooLong:
                    throw new PersonNameInvalidException(PersonNameValidationResultsEnum.LastNameTooLong);
            }

            PhoneValidationResultEnum phoneValidationResult = ValidatePhoneNumber(person.Phone);
            if (phoneValidationResult != PhoneValidationResultEnum.NumberOk)
            {
                throw new PhoneNumberInvalidException(phoneValidationResult);
            }

            return true;
        }

        public PersonNameValidationResultsEnum ValidateName(string name)
        {
            if (name.IsBlank())
            {
                return PersonNameValidationResultsEnum.NameBlank;
            }

            if (name.Length > NameMaxLength)
            {
                return PersonNameValidationResultsEnum.NameTooLong;
            }

            return PersonNameValidationResultsEnum.Ok;
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
        }
    }
}