namespace SalesApp.Core.Framework
{
    /// <summary>
    /// This interface defines how the system will be able to get device/user specific locale settings.
    /// </summary>
    public interface ICulture
    {
        /// <summary>
        /// This method returns the standardized Date Format string for short dates.
        /// </summary>
        /// <returns>Standard format for Short Dates</returns>
        string GetShortDateFormat();

        /// <summary>
        /// This method returns the standardized Date Format string for long dates.
        /// </summary>
        /// <returns>Standard format for long Dates</returns>
        string GetLongDateFormat();

        /// <summary>
        /// This method returns the standardized time format string for DateTime objects.
        /// </summary>
        /// <returns>Standard format for time</returns>
        string GetTimeFormat();

        /// <summary>
        /// This method retrieves the locale of the user (based on device)
        /// </summary>
        /// <returns>User locale</returns>
        string GetLocale();
    }
}