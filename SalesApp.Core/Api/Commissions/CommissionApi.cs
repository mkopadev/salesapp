using SalesApp.Core.Enums.MultiCountry;

namespace SalesApp.Core.Api.Commissions
{
    public class CommissionApi : ApiBase
    {
        public CommissionApi(string apiRelativePath) : base(apiRelativePath)
        {
        }

        public CommissionApi(string apiRelativePath, LanguagesEnum lang, CountryCodes countryCode)
            : base(apiRelativePath, lang, countryCode)
        {
        }
    }
}