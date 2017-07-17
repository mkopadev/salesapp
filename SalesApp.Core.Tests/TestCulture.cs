using SalesApp.Core.Framework;

namespace SalesApp.Core.Tests
{
    public class TestCulture :ICulture
    {
        public string GetShortDateFormat()
        {
            return "dd/MMM/yyyy";
        }

        public string GetLongDateFormat()
        {
            return "dd/MMM/yyyy HH:mm:ss.mmm";
        }

        public string GetTimeFormat()
        {
            return "HH:mm:ss.mmm";
        }

        public string GetLocale()
        {
            return "en-KE";
        }
    }
}
