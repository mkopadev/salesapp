using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.OtaSettings;
using SalesApp.Core.BL.Models.OtaSettings;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.OtaSettings
{
    /// <summary>
    /// Class that helps perform database related tasks for the OTA Settings
    /// </summary>
    public class LocalOtaService
    {
        /// <summary>
        /// The registration settings group
        /// </summary>
        public const string Registration = "Registration";

        /// <summary>
        /// The stats settings group
        /// </summary>
        public const string Stats = "Stats";

        /// <summary>
        /// The support settings group
        /// </summary>
        public const string Support = "Support";

        /// <summary>
        /// The communication settings group
        /// </summary>
        public const string Communication = "Communication";

        /// <summary>
        /// The photos settings group
        /// </summary>
        public const string Photos = "Photos";

        /// <summary>
        /// The local settings group. For settings that don't come from the server
        /// </summary>
        public const string Local = "Local";

        /// <summary>
        /// Stock management settings
        /// </summary>
        public const string StockManagement = "StockManagement";

        /// <summary>
        /// A dictionary to be used as a cache
        /// </summary>
        private readonly Dictionary<string, string> _settingsCache = new Dictionary<string, string>();

        /// <summary>
        /// The logger to use for writing logs
        /// </summary>
        private ILog _logger;

        /// <summary>
        /// The settings controller
        /// </summary>
        private OtaSettingsController _settingsController;

        /// <summary>
        /// Gets the settings controller
        /// </summary>
        private OtaSettingsController SettingsController
        {
            get
            {
                this._settingsController = this._settingsController ?? new OtaSettingsController();
                return this._settingsController;
            }
        }

        /// <summary>
        /// Gets the logger instance
        /// </summary>
        private ILog Logger
        {
            get
            {
                if (this._logger == null)
                {
                    this._logger = Resolver.Instance.Get<ILog>();
                    this._logger.Initialize(this.GetType().FullName);
                }

                return this._logger;
            }
        }

        /// <summary>
        /// Write the setting to the database
        /// </summary>
        /// <param name="setting">The setting</param>
        /// <returns>A boolean task</returns>
        public async Task SetSettingsValue(OtaSetting setting)
        {
            await this.SetSettingsValue(setting.GroupName, setting.Name, setting.Value);
        }

        /// <summary>
        /// Write the setting to the database
        /// </summary>
        /// <param name="group">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="value">The setting value</param>
        /// <returns>A boolean task</returns>
        public async Task SetSettingsValue(string group, string name, object value)
        {
            string settingValue = value != null ? value.ToString() : string.Empty;
            OtaSetting newSetting = await this.GetExistingSetting(name, group, settingValue);
            List<OtaSetting> settings = new List<OtaSetting> { newSetting };
                await this.SaveSettings(settings);

            this.CacheSetting(newSetting);
        }

        /// <summary>
        /// Writes the setting into the database and updates if it exists
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="group">The group of the setting</param>
        /// <param name="value">The value of the setting</param>
        /// <returns>A boolean task</returns>
        private async Task<OtaSetting> GetExistingSetting(string name, string group, string value)
            {
            // if it is not yet in the database, save it a fresh else update the existing
            OtaSetting existing = await this.GetOtaSetting(group, name);
            if (existing == null)
            {
                return new OtaSetting { GroupName = group, Name = name, Value = value };
            }

            existing.Value = value;
                existing.Modified = DateTime.Now;
            return existing;
        }

        /// <summary>
        /// Gets the setting as an integer
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into an integer</exception>
        public int GetInt(string groupName, string name, int defaultValue = 0)
        {
            return int.Parse(this.GetString(groupName, name, defaultValue));
        }

        /// <summary>
        /// Gets the setting as a boolean value
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into an boolean</exception>
        public bool GetBool(string groupName, string name, bool defaultValue = false)
        {
            string dbval = this.GetString(groupName, name, defaultValue);

            return bool.Parse(dbval);
        }

        /// <summary>
        /// Gets the setting as a double precision number
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into an double</exception>
        public double GetDouble(string groupName, string name, string defaultValue = "0.0")
        {
            return double.Parse(this.GetString(groupName, name, defaultValue));
        }

        /// <summary>Gets the setting as the give ENUM type</summary>
        /// <typeparam name="T">The type of ENUM to which to convert the setting</typeparam>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into the given ENUM</exception>
        public T GetEnum<T>(string groupName, string name, object defaultValue = null) where T : struct
        {
            string dbval = this.GetString(groupName, name, defaultValue);
            return dbval.ToEnumValue<T>();
        }

        /// <summary>
        /// Get the setting as a date time value
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into an date time</exception>
        public DateTime GetDateTime(string groupName, string name, object defaultValue = null)
        {
            string val = this.GetString(groupName, name, defaultValue);
            DateTime date;
            if (!DateTime.TryParse(val, out date))
            {
                if (defaultValue == null)
                {
                    defaultValue = default(DateTime);
                }

                return (DateTime)defaultValue;
            }

            return date;
        }

        /// <summary>
        /// Gets the setting as a time span value
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        /// <exception cref="InvalidCastException">If the value cannot be converted into an time span</exception>
        public TimeSpan GetSpan(string groupName, string name, object defaultValue = null)
        {
            if (defaultValue == null)
            {
                defaultValue = default(TimeSpan);
            }

            return this.GetString(groupName, name, defaultValue).ToTimeSpan((TimeSpan)defaultValue);
        }

        /// <summary>
        /// Gets the setting as a complex object T. This method is not type-safe, use with caution
        /// </summary>
        /// /<typeparam name="T">The complex object to which to convert the setting</typeparam>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing s found in the database</param>
        /// <returns>Returns the setting for the given name and group</returns>
        public T GetObject<T>(string groupName, string name, object defaultValue = null) where T : class
        {
            return JsonConvert.DeserializeObject<T>(this.GetString(groupName, name, defaultValue));
        }

        /// <summary>
        /// Gets the setting from the database or from the cache
        /// </summary>
        /// <param name="groupName">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value if nothing can be found</param>
        /// <returns>The setting value as a string</returns>
        public string GetString(string groupName, string name, object defaultValue = null)
        {
            string key = string.Format("{0}.{1}", groupName, name);

            if (this._settingsCache.ContainsKey(key))
            {
                return this._settingsCache[key];
            }

            //defaultValue = defaultValue == null ? EmptyObject : defaultValue.ToString();
            string defaultReturnValue = defaultValue == null ? null : defaultValue.ToString();

            this.Logger.Debug("Reading setting value for " + name);

            //var defaultSetting = new OtaSetting { Value = defaultValue.ToString() };
            var defaultSetting = new OtaSetting { Value = defaultReturnValue };

            OtaSetting setting = AsyncHelper.RunSync(
                async () =>
                await this.GetOtaSetting(
                    groupName,
                    name,
                    defaultSetting));

            //string returnValue = setting.Id == default(Guid) ? defaultValue.ToString() : setting.Value;
            string returnValue = setting.Id == default(Guid) ? defaultReturnValue : setting.Value;

            // Lets cache our setting
            this._settingsCache.Add(key, returnValue);
            return returnValue;
        }

        /// <summary>
        /// Reads and returns a setting from the database
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <param name="name">The setting name</param>
        /// <param name="defaultValue">The default value to return if nothing cannot be found</param>
        /// <returns>The given setting</returns>
        private async Task<OtaSetting> GetOtaSetting(string groupName, string name, OtaSetting defaultValue = null)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

            try
            {
                this.Logger.Debug("Reading setting for " + name);
                OtaSetting setting =
                    await
                    this.SettingsController.GetSingleRecord(
                        criteriaBuilder.Add("GroupName", groupName).Add("Name", name));

                return setting.Id == default(Guid) ? defaultValue : setting;
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Cache the given setting
        /// </summary>
        /// <param name="setting">The setting to cache</param>
        public void CacheSetting(OtaSetting setting)
        {
            string key = string.Format("{0}.{1}", setting.GroupName, setting.Name);
            if (this._settingsCache.ContainsKey(key))
            {
                this._settingsCache[key] = setting.Value;
                return;
            }

            this._settingsCache.Add(key, setting.Value);
        }

        public async Task SaveSettings(List<OtaSetting> settings)
        {
           await this.SettingsController.SaveBulkAsync(settings);
        }
    }
}
