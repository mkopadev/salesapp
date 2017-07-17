namespace SalesApp.Core.Services.Connectivity
{
    /// <summary>
    /// This class represents a SMS Service, it allows a device to implement a specific SMS service.
    /// </summary>
    public interface ISmsService : ISmsServiceEventListener
    {
        
        /// <summary>
        /// This method sends a SMS
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool SendSms(string phoneNumber, string message);
    }
}