using System;

namespace SalesApp.Core.Services.RemoteServices
{
    public class ExceptionHandledEventArgs : EventArgs
    {
        public ExceptionHandledEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}