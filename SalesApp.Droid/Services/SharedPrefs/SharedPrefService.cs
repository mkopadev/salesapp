using Android.App;
using Android.Content;
using SalesApp.Core.Services.SharedPrefs;

namespace SalesApp.Droid.Services.SharedPrefs
{
    public class SharedPrefService : ISharedPrefService
    {
        private ISharedPreferences _sharedPreference;
        public SharedPrefService()
        {
            Initialize();
        }

        public void Initialize()
        {
            _sharedPreference = Application.Context.GetSharedPreferences("settings", FileCreationMode.Private);
        }

        public void Save(string key, string value)
        {
            var prefEditor = _sharedPreference.Edit();
            prefEditor.PutString(key, value);
            prefEditor.Commit();
        }

        public void Save(string key, bool value)
        {
            var prefEditor = _sharedPreference.Edit();
            prefEditor.PutBoolean(key, value);
            prefEditor.Commit();
        }

        public string Get(string key)
        {
            string value = _sharedPreference.GetString(key, null);
            return value;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            bool value = _sharedPreference.GetBoolean(key, defaultValue);
            return value;
        }

        public void Remove(string key)
        {
            var prefEditor = _sharedPreference.Edit();
            prefEditor.Remove(key);
            prefEditor.Commit();
        }
    }
}