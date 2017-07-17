using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace SalesApp.Core.Converters
{
    /// <summary>
    /// This class is used by MvvmCross via reflection, do not delete. ^Musyoka
    /// </summary>
    public class DateTimeValueConverter : MvxValueConverter<DateTime, string>
    {
        protected override string Convert(DateTime value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            try
            {
                if (value == default(DateTime))
                {
                    return string.Empty;
                }

                return value.ToString("dd/MM/yyyy");
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }
    }
}