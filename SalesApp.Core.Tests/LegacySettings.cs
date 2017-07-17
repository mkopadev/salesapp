using System;
using Mkopa.Core.Enums.MultiCountry;
using Mkopa.Core.Services.Settings;

namespace MKopa.Core.Tests
{
    using Mkopa.Core.Enums.OtaSettings;

    public class LegacySettings : ILegacySettings
    {
        
        public CountryCodes DsrCountryCode
        {
            get
            {
                return CountryCodes.KE;
            }
            set
            {
                
            }
        }
        public string ApiAuthorizationHeader { get; set; }

        public LanguagesEnum DsrLanguage
        {
            get
            {
                return LanguagesEnum.EN;
            }
            set { }
        }
        public string DsrPhone { get; set; }
        public DateTime? LastMessageFetchDate { get; set; }
        public int OverDueDaysMeasure { get; set; }
        public int ExpirePeriodDays { get; set; }
        public int OfflineLoginAttempts { get; set; }
        public TimeSpan DefaultApiTimeout { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        public void Save(string name, string value, GroupNames groupName = default(GroupNames))
        {
            throw new NotImplementedException();
        }

        public string Load(string name, GroupNames groupName = default(GroupNames))
        {
            throw new NotImplementedException();
        }

        private DayOfWeek _startOfWeek = DayOfWeek.Sunday;
        public DayOfWeek StartOfWeek
        {
            get { return _startOfWeek; }
            set { _startOfWeek = value; }
        }

        public int IndividualStatsLookBackMonths { get { return 0; }}
        public int RankingSummarizedValidityMinutes { get { return 0; } }
        public int RankingListValidityMinutes { get { return 100000; } }
        public int SalesStatsValidityMinutes { get { return 0; } }


        public int UnitsInfoValidityMinutes
        {
            get { throw new NotImplementedException(); }
        }

        public int DefaultCacheTimeout { get; set; }
    }
}