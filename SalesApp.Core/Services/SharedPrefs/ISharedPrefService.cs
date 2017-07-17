namespace SalesApp.Core.Services.SharedPrefs
{
    public interface ISharedPrefService
    {
        void Initialize();

        void Save(string key, string value);

        void Save(string key, bool value);

        string Get(string key);

        bool GetBool(string key, bool defaultValue = false);

        void Remove(string key);
    }
}