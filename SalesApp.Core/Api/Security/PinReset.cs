using System.Threading.Tasks;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Exceptions.Validation.People;
using SalesApp.Core.Validation;

namespace SalesApp.Core.Api.Security
{
    /// <summary>
    /// Class with functionality required to reset pin
    /// </summary>
    public class PinResetOperations : ApiBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Context that initialized the object</param>
        public PinResetOperations()
            : base("resetpin")
        {
            
        }

        /// <summary>
        /// Method that verifies a received phone number and makes the API call that requests the server for the associated PIN to be reset.
        /// </summary>
        /// <param name="phoneNumber">Phone number whose PIN is to be reset</param>
        /// <returns>A ServerResponseDto object with results of the API call</returns>
        public async Task<ServerResponseDto> ResetPinAsync(string phoneNumber)
        {
            
            PhoneValidationResultEnum result = new PeopleDetailsValidater().ValidatePhoneNumber(phoneNumber);
            if(result != PhoneValidationResultEnum.NumberOk)
            {
                throw  new PhoneNumberInvalidException(result);
            }

            return await this.PostObjectAsync<ServerResponseDto>(new { PhoneNumber = phoneNumber });
        }
    }
}