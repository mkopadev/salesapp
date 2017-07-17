using SalesApp.Core.Enums.MultiCountry;

namespace SalesApp.Core.Api.Modules.Calculator
{
    public class CalculatorProductApi : ApiBase
    {
        public CalculatorProductApi(string apiRelativePath) : base(apiRelativePath)
        {
        }

        public CalculatorProductApi(string apiRelativePath, LanguagesEnum lang, CountryCodes countryCode) : base(apiRelativePath, lang, countryCode)
        {
        }
    }
}