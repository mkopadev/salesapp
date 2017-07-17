using System;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.OtaSettings;

namespace SalesApp.Core.Services.Settings
{
    /// <summary>
    /// Class holds all the configuration settings that the app uses
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Static thread-safe singleton initialization
        /// </summary>
        private static readonly Lazy<Settings> Lazy = new Lazy<Settings>(() => new Settings());

        /// <summary>
        /// Service to help get and set settings from the database
        /// </summary>
        private readonly LocalOtaService _localOtaService;

        /// <summary>
        /// Prevents a default instance of the <see cref="Settings"/> class from being created.
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        private Settings()
        {
            this._localOtaService = new LocalOtaService();
        }

        /// <summary>
        /// Gets a publicly available settings singleton
        /// </summary>
        public static Settings Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// Gets or sets the DSR's country code
        /// </summary>
        public CountryCodes DsrCountryCode
        {
            get
            {
                return this._localOtaService.GetEnum<CountryCodes>(LocalOtaService.Local, "DsrCountryCode", CountryCodes.KE);
            }

            set
            {
                this.SetValue(LocalOtaService.Local, "DsrCountryCode", value);
            }
        }

        /// <summary>
        /// Gets or sets the DSR's selected language
        /// </summary>
        public LanguagesEnum DsrLanguage
        {
            get
            {
                if (this.DsrCountryCode == CountryCodes.TZ)
                {
                    return LanguagesEnum.SW;
                }

                return this._localOtaService.GetEnum<LanguagesEnum>(LocalOtaService.Local, "DsrLanguage", LanguagesEnum.EN);
            }

            set
            {
                this.SetValue(LocalOtaService.Local, "DsrLanguage", value);
            }
        }

        /// <summary>
        /// Gets or sets the expiry period in days
        /// </summary>
        public int ExpirePeriodInDays
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Support, "ExpirePeriodInDays", 4);
            }

            set
            {
                this.SetValue(LocalOtaService.Support, "ExpirePeriodInDays", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the number of allowed offline login attempts
        /// </summary>
        public int OfflineLoginAttempts
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Communication, "OfflineLoginAttempts", 4);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "OfflineLoginAttempts", value);
            }
        }

        /// <summary>
        /// Gets or sets the APIi time out value
        /// </summary>
        public TimeSpan DefaultApiTimeout
        {
            get
            {
                return this._localOtaService.GetSpan(LocalOtaService.Local, "DefaultApiTimeout");
            }

            set
            {
                this.SetValue(LocalOtaService.Local, "DefaultApiTimeout", value);
            }
        }

        /// <summary>
        /// Gets or sets the start of week day
        /// </summary>
        public DayOfWeek StartOfWeek
        {
            get
            {
                return this._localOtaService.GetEnum<DayOfWeek>(LocalOtaService.Communication, "StartOfWeek", (int)DayOfWeek.Monday);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "StartOfWeek", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the number of months for which to fetch individual stats records
        /// </summary>
        public int IndividualStatsLookBackMonths
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "IndividualStatsLookBackMonths", 6);
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "IndividualStatsLookBackMonths", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the summarized ranking validity in minutes
        /// </summary>
        public int RankingSummarizedValidityMinutes
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "RankingSummarizedValidityMinutes");
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "RankingSummarizedValidityMinutes", value);
            }
        }

        /// <summary>
        /// Gets or sets the ranking list validity in minutes
        /// </summary>
        public int RankingListValidityMinutes
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "RankingListValidityMinutes");
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "RankingListValidityMinutes", value);
            }
        }

        /// <summary>
        /// Gets or sets the sales stats validity in minutes
        /// </summary>
        public int SalesStatsValidityMinutes
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "SalesStatsValidityMinutes");
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "SalesStatsValidityMinutes", value);
            }
        }

        /// <summary>
        /// Gets or sets the validity for units info in minutes
        /// </summary>
        public int UnitsInfoValidityMinutes
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "UnitsInfoValidityMinutes");
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "UnitsInfoValidityMinutes", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum number of fallback (SMS) re-tries for customer registration
        /// </summary>
        public int MaxFallbackRetries
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Communication, "MaxFallbackRetries", 3);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "MaxFallbackRetries", value);
            }
        }

        /// <summary>
        /// Gets or sets a JSON representation of the available products
        /// </summary>
        [Setting(EmptySets = "[],{},0")]
        public string Products
        {
            get
            {
                string defaultVal = "[]";
                return this._localOtaService.GetString(LocalOtaService.Registration, "Products", defaultVal);
            }

            set
            {
                this.SetValue(LocalOtaService.Registration, "Products", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the SMS short code
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string SMSShortCode
        {
            get
            {
                string shortCode = string.Empty;

                return this._localOtaService.GetString(LocalOtaService.Registration, "SMSShortCode", shortCode);
            }

            set
            {
                this.SetValue(LocalOtaService.Registration, "SMSShortCode", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the last time the messages were fetched
        /// </summary>
        public DateTime LastMessageFetchDate
        {
            get
            {
                return this._localOtaService.GetDateTime(LocalOtaService.Local, "LastMessageFetchDate");
            }

            set
            {
                this.SetValue(LocalOtaService.Local, "LastMessageFetchDate", value);
            }
        }

        /// <summary>
        /// Gets or sets the users phone number
        /// </summary>
        public string DsrPhone
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Registration, "DsrPhone");
            }

            set
            {
                this.SetValue(LocalOtaService.Registration, "DsrPhone", value);
            }
        }

        /// <summary>
        /// Gets or sets a date that helps the server send only data that has changed
        /// </summary>
        public string ServerTimeStamp
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "ServerTimeStamp");
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "ServerTimeStamp", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating interval of location updates
        /// </summary>
        public int LocationInfoRetrievalInterval
        {
            get
            {
                int defaultValue = 5;

                if (this.DsrCountryCode == CountryCodes.TZ)
                {
                    defaultValue = 100;
                }

                return this._localOtaService.GetInt(LocalOtaService.Communication, "LocationInfoRetrievalInterval", defaultValue);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "LocationInfoRetrievalInterval", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to enable location updates
        /// </summary>
        public bool EnableLocationInfo
        {
            get
            {
                return this._localOtaService.GetBool(LocalOtaService.Communication, "EnableLocationInfo", true);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "EnableLocationInfo", value);
            }
        }

        /// <summary>
        /// Gets or sets the location ProviderType
        /// </summary>
        public string ProviderType
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "ProviderType", "COURSE");
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "ProviderType", value);
            }
        }

        /// <summary>
        /// Gets or sets the Default timeout in minutes for cached objects.
        /// </summary>
        public int DefaultCacheTimeout
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Stats, "DefaultCacheTimeout", 1440);
            }

            set
            {
                this.SetValue(LocalOtaService.Stats, "DefaultCacheTimeout", value);
            }
        }

        /// <summary>
        /// Gets or sets the DSR support line
        /// </summary>
        public string DealerSupportLine
        {
            get
            {
                string defaulValue = string.Empty;

                return this._localOtaService.GetString(LocalOtaService.Communication, "DealerSupportLine", defaulValue);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "DealerSupportLine", value);
            }
        }

        /// <summary>
        /// Sets the setting value by saving it to the database
        /// </summary>
        /// <param name="group">The setting group</param>
        /// <param name="name">The setting name</param>
        /// <param name="value">The value for the setting</param>
        private void SetValue(string group, string name, object value)
        {
            AsyncHelper.RunSync(
                    async () => await this._localOtaService.SetSettingsValue(group, name, value));
        }

        /// <summary>
        /// Gets or sets the photo enabled feature
        /// </summary>
        public int PhotoFeatureEnabled
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Registration, "PhotoFeatureEnabled", 1);
            }

            set
            {
                this.SetValue(LocalOtaService.Registration, "PhotoFeatureEnabled", value);
            }
        }

        /// <summary>
        /// Gets or sets the customer wizard process flow definition
        /// </summary>
        public string CustomerWizard
        {
            get
            {
                return _localOtaService.GetString(LocalOtaService.Support, "CustomerWizard");
            }

            set
            {
                SetValue(LocalOtaService.Support, "CustomerWizard", value);
            }
        }

        /// <summary>
        /// Gets or sets the dsr wizard process flow definition
        /// </summary>
        public string DsrWizard
        {
            get
            {
                return _localOtaService.GetString(LocalOtaService.Support, "DsrWizard");
            }

            set
            {
                SetValue(LocalOtaService.Support, "DsrWizard", value);
            }
        }

        public string VideosDirectory
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "VideosDirectory", "/SalesApp/Videos/");
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "VideosDirectory", value);
            }
        }

        public string GoogleAnalyticsTrackingId
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "GoogleAnalyticsTrackingId", "UA-76974233-1");
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "GoogleAnalyticsTrackingId", value);
            }
        }

        public int GoogleAnalyticsDispatchPeriod
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Communication, "GoogleAnalyticsDispatchPeriod", 10);
            }

            set
            {
                this.SetValue(LocalOtaService.Communication, "GoogleAnalyticsDispatchPeriod", value);
            }
        }

        public string AzureSasToken
        {
            get
            {
                string defaultValue = string.Empty;

                return this._localOtaService.GetString(LocalOtaService.Photos, "AzureSASToken", defaultValue);
            }

            set
            {
                this.SetValue(LocalOtaService.Photos, "AzureSASToken", value);
            }
        }

        public int CustomerPhotoWidthScale
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Photos, "CustomerPhotoWidthScale", 300);
            }

            set
            {
                SetValue(LocalOtaService.Photos, "CustomerPhotoWidthScale", value);
            }
        }

        public int CustomerPhotoHeightScale
        {
            get
            {
                return this._localOtaService.GetInt(LocalOtaService.Photos, "CustomerPhotoHeightScale", 300);
            }

            set
            {
                SetValue(LocalOtaService.Photos, "CustomerPhotoHeightScale", value);
            }
        }

        public string ReasonForReturn
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.StockManagement, "ReasonForReturn");
            }

            set
            {
                SetValue(LocalOtaService.StockManagement, "ReasonForReturn", value);
            }
        }

        public string InsightsDebugApiKey
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "InsightsDebugApiKey");
            }

            set
            {
                SetValue(LocalOtaService.Communication, "InsightsDebugApiKey", value);
            }
        }

        public string InsightsProductionApiKey
        {
            get
            {
                return this._localOtaService.GetString(LocalOtaService.Communication, "InsightsProductionApiKey");
            }

            set
            {
                SetValue(LocalOtaService.Communication, "InsightsProductionApiKey", value);
            }
        }
    }
}