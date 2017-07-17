using System;
using Android.Content;

namespace SalesApp.Droid.Services.Notification
{
    public class DestinationInformation
    {
        public Type ActivityType { get; set; }

        public Intent ContentIntent { get; set; }
    }
}