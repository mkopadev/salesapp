using Java.Text;
using Java.Util;
using SalesApp.Core.Framework;

namespace SalesApp.Droid.Framework
{
    public class Culture : ICulture
    {
        private readonly Locale androidLocale = Locale.Default;
        private readonly DateFormat shortDateFormat = Android.Text.Format.DateFormat.GetDateFormat(SalesApplication.Instance.ApplicationContext);
        private readonly DateFormat longDateFormat = Android.Text.Format.DateFormat.GetLongDateFormat(SalesApplication.Instance.ApplicationContext);

        public string GetShortDateFormat()
        {
            return ((SimpleDateFormat)shortDateFormat).ToLocalizedPattern();
        }

        public string GetLongDateFormat()
        {
            return ((SimpleDateFormat)longDateFormat).ToLocalizedPattern();
        }

        public string GetTimeFormat()
        {
            return "HH:mm:ss";
        }

        public string GetLocale()
        {
            var netLanguage = androidLocale.ToString().Replace("_", "-");
            return netLanguage.ToLower();
        }

        public string DayMonthYearFormat()
        {
            return "d/M/yyyy";
        }
    }
}