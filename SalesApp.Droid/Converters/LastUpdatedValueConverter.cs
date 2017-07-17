using System;
using System.Globalization;
using Android.App;
using Android.Content;
using MvvmCross.Platform.Converters;

namespace SalesApp.Droid.Converters
{
    /// <summary>
    /// This class is used by MvvmCross via reflection, do not delete. ^Musyoka
    /// </summary>
    public class LastUpdatedValueConverter : MvxValueConverter<DateTime, string>
    {
        private Context _context = Application.Context;

        protected override string Convert(DateTime value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            string lastUpdated = _context.GetString(Resource.String.last_updated);

            if (value == default(DateTime))
            {
                string statsNever = _context.GetString(Resource.String.stats_never);

                return string.Format(lastUpdated, string.Empty, statsNever, string.Empty).ToUpper();
            }

            TimeSpan timeSpan = DateTime.Now - value;
            string ago = _context.GetString(Resource.String.stats_ago);

            if (timeSpan.Days > 0)
            {
                string days = _context.GetString(Resource.String.days);
                return string.Format(lastUpdated, timeSpan.Days, days, ago).ToUpper();
            }

            if (timeSpan.Hours > 0)
            {
                string hours = _context.GetString(Resource.String.hours);
                return string.Format(lastUpdated, timeSpan.Hours, hours, ago).ToUpper();
            }

            string lessThanAnHour = _context.GetString(Resource.String.less_than_an_hour);
            return string.Format(lastUpdated, lessThanAnHour, string.Empty, ago).ToUpper();
        }
    }
}