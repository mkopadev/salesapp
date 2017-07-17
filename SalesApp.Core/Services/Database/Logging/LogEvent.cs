using System;

namespace SalesApp.Core.Services.Database.Logging
{
    public class LogEvent : EventArgs
    {
        public LogEvent(string information,bool isError)
        {
            Information = information;
            IsError = isError;
        }

        public string Information { get; set; }
        public bool IsError { get; set; }
    }
}