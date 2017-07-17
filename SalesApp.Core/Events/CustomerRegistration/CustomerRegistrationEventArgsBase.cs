using System;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Events.CustomerRegistration
{
    public abstract class CustomerRegistrationEventArgsBase : EventArgs
    {
        public CustomerRegistrationEventArgsBase(DataChannel channel)
        {
            Channel = channel;
        }

        public DataChannel Channel { get; set; }
    }
}