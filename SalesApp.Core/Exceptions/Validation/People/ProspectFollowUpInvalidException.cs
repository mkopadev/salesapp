using System;
using SalesApp.Core.Enums.People;

namespace SalesApp.Core.Exceptions.Validation.People
{
    public class ProspectFollowUpInvalidException : Exception
    {
        public ProspectFollowUpValidationResultsEnum ValidationResult { get; private set; }
        public ProspectFollowUpInvalidException(ProspectFollowUpValidationResultsEnum validationResult)
        {
            this.ValidationResult = validationResult;
        }
    }
}
