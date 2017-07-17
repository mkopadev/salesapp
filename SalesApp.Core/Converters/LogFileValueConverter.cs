using System;
using System.Globalization;
using System.IO;
using MvvmCross.Platform.Converters;

namespace SalesApp.Core.Converters
{
    public class LogFileValueConverter : MvxValueConverter<string, string>
    {
        /// <summary>
        /// This class is used by MvvmCross via reflection, do not delete. ^Musyoka
        /// </summary>
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            string fileName = Path.GetFileNameWithoutExtension(value);
            try
            {
                DateTime date = DateTime.ParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture);
                return date.ToString("dd/MM/yyyy");
            }
            catch (Exception)
            {
                return fileName;
            }
        }
    }
}
