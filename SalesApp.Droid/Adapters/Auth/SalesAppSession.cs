using System;
using Android.Content;
using SalesApp.Core.Auth;

namespace SalesApp.Droid.Adapters.Auth
{
    public class SalesAppSession : ISalesAppSession
    {
        
        private ISharedPreferences _preferences;

        private readonly string _userSession = "user_session";
        private readonly string _firstName = "first_name";
        private readonly string _lastName = "last_name";
        private readonly string _userId = "user_id";
        private readonly string _userHash = "user_hash";

        public void Clear()
        {
            FirstName = null;
            LastName = null;
            UserHash = null;
            UserId = Guid.Empty;
        }

        public SalesAppSession()
        {
            _preferences = SalesApplication.Instance.GetSharedPreferences(_userSession, FileCreationMode.Private);
        }

        public string FirstName
        {
            get { return _preferences.GetString(_firstName, String.Empty); }
            set { SetPreference(_firstName, value); }
        }

        public string LastName
        {
            get { return _preferences.GetString(_lastName, String.Empty); }
            set { SetPreference(_lastName, value); }
        }

        public Guid UserId
        {
            get
            {

                Guid userId;
                if (Guid.TryParse(_preferences.GetString(_userId, String.Empty), out userId))
                {
                    return userId;
                }

                return Guid.Empty;

            }
            set { SetPreference(_userId, value.ToString()); }
        }

        public string UserHash
        {
            get { return _preferences.GetString(_userHash, String.Empty); }
            set { SetPreference(_userHash, value); }
        }

        private void SetPreference(string key, string value)
        {
            var preferencesEditor = _preferences.Edit();
            preferencesEditor.PutString(key, value);
            preferencesEditor.Commit();
        }
    }
}