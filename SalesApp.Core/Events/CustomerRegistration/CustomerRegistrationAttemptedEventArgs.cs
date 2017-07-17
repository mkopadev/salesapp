using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Events.CustomerRegistration
{
    public class CustomerRegistrationAttemptedEventArgs : CustomerRegistrationEventArgsBase
    {
        public CustomerRegistrationAttemptedEventArgs(DataChannel channel, int currentAttempt, int maxAttempts) : base(channel)
        {
            this.CurrentAttempt = currentAttempt;
            this.MaxAttempts = maxAttempts;
        }

        public int CurrentAttempt { get; set; }

        public int MaxAttempts { get; set; }
    }
}