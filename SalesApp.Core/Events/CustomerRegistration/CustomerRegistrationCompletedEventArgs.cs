using System;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Events.CustomerRegistration
{
    public class CustomerRegistrationCompletedEventArgs : CustomerRegistrationEventArgsBase
    {
        public CustomerRegistrationCompletedEventArgs(DataChannel channel, bool succeeded) : base(channel)
        {
            Succeeded = succeeded;
        }

        public bool Succeeded { get; set; }

        public Guid RegistrationId { get; set; }
    }
}
