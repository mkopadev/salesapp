using System.IO;
using SalesApp.Core.Services.Interfaces;

namespace SalesApp.Core.Tests
{
    public class StorageService : IStorageService
    {
        private string SettingsPath
        {
            get
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\SalesApp\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string GetPathForFileAsync(string file)
        {
            return Path.Combine(this.SettingsPath, file);
        }

        public string ReadValue(string name, object defaultValue)
        {
            string settingsFile = SettingsPath + name;
            if (!File.Exists(settingsFile))
            {
                if (defaultValue != null)
                {
                    return defaultValue.ToString();
                }

                return string.Empty;
            }

            string value = File.ReadAllText(this.SettingsPath + name);
            if (string.IsNullOrEmpty(value) && defaultValue != null)
            {
                return defaultValue.ToString();
            }

            return value;
        }

        public bool WriteValue(string name, object value)
        {
            string settingsFile = this.SettingsPath + name;
            File.WriteAllText(settingsFile, value.ToString());
            return true;
        }
    }
}
