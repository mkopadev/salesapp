using System;
using Android.Content;
using SalesApp.Core.Logging;

namespace SalesApp.Droid.Logging
{
    public class LogSettings : ILogSettings
    {
        private readonly string logSettings = "log_settings";
        private readonly string fileExtensiveLogs = "file_extensive_logs";
        private readonly string lastOnTimestamp = "last_on_timestamp";
        private readonly int maxExtensiveLoggingSeconds = 60 * 60 * 4;
        private ISharedPreferences preferences;

        public LogSettings()
        {
            this.preferences = SalesApplication.Instance.GetSharedPreferences(this.logSettings, FileCreationMode.Private);
        }

        public bool FileExtensiveLogs
        {
            get
            {
                if (this.IsOverdue())
                {
                    FileExtensiveLogs = false;
                }

                return this.preferences.GetBoolean(this.fileExtensiveLogs, false);
            }

            set
            {
                var preferencesEditor = this.preferences.Edit();
                preferencesEditor.PutBoolean(this.fileExtensiveLogs, value);
                if (value)
                {
                    // also update last time we turned it on. To help automatically turn off after 4 hrs
                    LastOnTimestamp = DateTime.Now.ToString();
                }

                preferencesEditor.Commit();
            }
        }

        public string LastOnTimestamp
        {
            get
            {
                return this.preferences.GetString(this.lastOnTimestamp, default(DateTime).ToString());
            }

            set
            {
                var preferencesEditor = this.preferences.Edit();
                preferencesEditor.PutString(this.lastOnTimestamp, value);
                preferencesEditor.Commit();
            }
        }

        private bool IsOverdue()
        {
            DateTime lastSwitchOn;

            DateTime.TryParse(LastOnTimestamp, out lastSwitchOn);

            // get seconds between then an now
            TimeSpan timeSpan = DateTime.Now - lastSwitchOn;

            bool on = this.preferences.GetBoolean(this.fileExtensiveLogs, false);

            return (timeSpan.TotalSeconds >= maxExtensiveLoggingSeconds) && on;
        }
    }
}