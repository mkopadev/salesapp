using System;
using System.Globalization;
using Android.Text;
using MvvmCross.Platform.Converters;

namespace SalesApp.Droid.Converters
{
    /// <summary>
    /// This class is used by MvvmCross via reflection, do not delete. ^Musyoka
    /// </summary>
    public class HtmlValueConverter : MvxValueConverter<string, ISpanned>
    {
        protected override ISpanned Convert(string value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            var spanned =  Html.FromHtml(value);
            return spanned;
        }
    }
}