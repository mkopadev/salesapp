using System;
using SalesApp.Core.Enums.Validation;

namespace SalesApp.Core.Exceptions.Validation.People
{
    public class PhoneNumberInvalidException : Exception
    {
        public PhoneValidationResultEnum ValidationResult { get; private set; }
        public PhoneNumberInvalidException(PhoneValidationResultEnum validationResult)
        {
            this.ValidationResult = validationResult;
        }

        
    }
}