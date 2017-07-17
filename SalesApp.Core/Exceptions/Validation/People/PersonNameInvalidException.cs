using System;
using SalesApp.Core.Enums.People;

namespace SalesApp.Core.Exceptions.Validation.People
{
    public class PersonNameInvalidException : Exception
    {
        public PersonNameValidationResultsEnum ValidationResults { get; private set; }
        public PersonNameInvalidException(PersonNameValidationResultsEnum validationResults)
        {
            this.ValidationResults = validationResults;
        }
    }
}