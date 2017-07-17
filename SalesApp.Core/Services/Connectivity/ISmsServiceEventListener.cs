namespace SalesApp.Core.Services.Connectivity
{
    public interface ISmsServiceEventListener
    {
        /// <summary>
        /// This event signals when a SMS Service failed to sent the SMS. Listeners can subscribe when they need to be informed.
        /// </summary>
        event SmsServiceCore.SmsSentEventHandler SmsSent;
        /// <summary>
        /// This event signals when a SMS Service got confirmation the SMS has been received. Listeners can subscribe when they need to be informed.
        /// </summary>
        event SmsServiceCore.SmsReceivedEventHandler SmsReceived;
        /// <summary>
        /// This event signals when a SMS Service got confirmation the SMS sending has failed. Listeners can subscribe when they need to be informed.
        /// </summary>
        event SmsServiceCore.SmsFailedEventHandler SmsFailed; 
    }
}