using System;
using System.Globalization;
using Android.Views;
using MvvmCross.Platform.Converters;

namespace SalesApp.Droid.Converters
{
    /// <summary>
    /// This class is used by MvvmCross via reflection, do not delete. ^Musyoka
    /// </summary>
    public class InvertedVisibilityValueConverter : MvxValueConverter<bool, ViewStates>
    {
        protected override ViewStates Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? ViewStates.Gone : ViewStates.Visible;
        }

        protected override bool ConvertBack(ViewStates value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == ViewStates.Gone)
            {
                return true;
            }

            return false;
        }
    }
}