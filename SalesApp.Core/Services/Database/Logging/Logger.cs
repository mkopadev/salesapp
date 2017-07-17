using System;

namespace SalesApp.Core.Services.Database.Logging
{
    class Logger
    {
        public event EventHandler<LogEvent> EventOccured; 
        public void Log(string information,bool isError)
        {
            EventOccured?.Invoke(this,new LogEvent(information,isError));
        }
    }
}