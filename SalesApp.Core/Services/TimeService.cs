using System;

namespace SalesApp.Core.Services
{
    /// <summary>
    /// This service exposes Date and Time related functionality.
    /// </summary>
    public class TimeService : ITimeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeService"/> class.
        /// Private constructor to force using the Get method.
        /// </summary>
        private TimeService()
        {
        }

        /// <summary>
        /// Returns a new instance of the TimeService.
        /// </summary>
        /// <returns>New Instance of the TimeService</returns>
        public static ITimeService Get()
        {
            return new TimeService();
        }

        /// <summary>
        /// Retrieves the Current DateTime from the system.
        /// </summary>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

/*        /// <summary>
        /// Returns first date of the month or laste date of the month
        /// </summary>
        /// <param name="month">Name of the month that  you want to get date</param>
        /// <param name="day">Either first day/last day of the month</param>
        /// <returns></returns>
        public static string GetFirstOrLastDateMonth(string month, int day)
        {
            DateTime date = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture);
            if (day == 1)
            {
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                return date.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
            }
        }*/
    }

    /// <summary>
    /// This service exposes Date and Time related functionality.
    /// </summary>
    public interface ITimeService
    {
        /// <summary>
        /// Retrieves the Current DateTime from the system.
        /// </summary>
        DateTime Now { get; }
    }
}
