using System;
using System.Globalization;
using SalesApp.Core.Framework;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Extensions
{
    public static class DateTimeExtensions
    {
        private static ICulture _culture = Resolver.Instance.Get<ICulture>();

        public static string GetDateStandardFormat(this DateTime date)
        {
            return date.ToString(_culture.GetShortDateFormat());
        }

        public static string GetTimeStandardFormat(this DateTime time)
        {
            return time.ToString("hh:mm tt");
        }

        public static string ToShortDateTime(this DateTime date)
        {
            return date.ToString(_culture.GetShortDateFormat());
        }

        public static string ToLongDateTime(this DateTime date)
        {
            return date.ToString(_culture.GetLongDateFormat());
        }

        public static DateTime ToMidnight(this DateTime date)
        {
            date = date.AddMilliseconds(date.Millisecond * -1);
            date = date.AddMinutes(date.Minute * -1);
            date = date.AddSeconds(date.Second * -1);
            date = date.AddHours(date.Hour * -1);
            return date;
        }

        public static DateTime ToEndOfDay(this DateTime date)
        {
            return date.ToMidnight().AddDays(1).AddMilliseconds(-1);
        }

        public static double ToMilliSeconds(this DateTime date)
        {
            return date.ToUniversalTime()
                .Subtract
                (
                    new DateTime
                        (
                        1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc
                        )
                ).TotalMilliseconds;
        }

        public static long ToUnixEpochSeconds(this DateTime date)
        {
            double result = date.ToUniversalTime()
                .Subtract(new DateTime
                        (
                        1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc
                        )
                ).TotalSeconds;
            return Convert.ToInt64(Math.Floor(result));
        }

        public static string ToMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ToShortMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

    }
}