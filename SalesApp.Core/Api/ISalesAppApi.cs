using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesApp.Core.Api
{
    public interface ISalesAppApi
    {
        /*/// <summary>
        /// Login with the specified PIN.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="phone">The phone.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="isFirstLogin">if set to <c>true</c> [is first login].</param>
        /// <returns></returns>
        Task<UserDto> Login(string pin, string phone, string deviceId, bool isFirstLogin);*/

        Task<BL.Models.People.Person> CheckPerson(string phone);
        Task<StatusDto> RegisterProspect(ProspectDto prospect);
        Task<StatusDto> RegisterCustomer(CustomerDto customer);
        Task<List<ProductDto>> GetProducts(string dsrPhone);
        Task<List<MessageDto>> GetMessages(DateTime sinceDate);
        Task<CustomerRegistrationStatusDto> GetRegistrationSteps(string phoneNumber);
    }
}