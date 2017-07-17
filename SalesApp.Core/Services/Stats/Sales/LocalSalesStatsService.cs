using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.BL.Models.Stats.Sales;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.Services.Stats.Sales
{
    public class LocalSalesStatsService
    {
        private readonly ILog _logger = LogManager.Get(typeof(LocalSalesStatsService));
        private StatsController _statsController;
        private DayOfWeek? _startDayOfWeek;

        public async Task<int> GetSalesTodayAsync()
        {
            List<Stat> stats = await this.GetStatsInRange(DateTime.Now, DateTime.Now);
            return this.Aggregate(stats, DateTime.Now, DateTime.Now, Period.Day).Sales;
        }

        public Tuple<DateTime, DateTime> GetWeekDayBelongsTo(DateTime date)
        {
            const int daysInWeek = 6;
            int daysToWeekStart = 0;

            DayOfWeek dayOfWeekToTest = date.DayOfWeek;
            if (this.StartDayOfWeek == null)
            {
                return new Tuple<DateTime, DateTime>(default(DateTime), default(DateTime));
            }

            if (dayOfWeekToTest == this.StartDayOfWeek.Value)
            {
                daysToWeekStart = 0;
            }
            else if (dayOfWeekToTest > this.StartDayOfWeek.Value)
            {
                daysToWeekStart = this.StartDayOfWeek.Value - dayOfWeekToTest;
            }
            else if (dayOfWeekToTest < this.StartDayOfWeek.Value)
            {
                daysToWeekStart = (int)dayOfWeekToTest + (daysInWeek + 1) - (int)this.StartDayOfWeek.Value;
                daysToWeekStart *= -1;
            }

            DateTime startOfWeekDate = date.AddDays(daysToWeekStart);
            return new Tuple<DateTime, DateTime>(startOfWeekDate, startOfWeekDate.AddDays(daysInWeek));
        }

        public async Task<int> GetSalesThisWeekAsync()
        {
            Tuple<DateTime, DateTime> thisWeek = this.GetWeekDayBelongsTo(DateTime.Now);
            List<Stat> stats = await this.GetStatsInRange(thisWeek.Item1, thisWeek.Item2);
            return this.Aggregate(stats, thisWeek.Item1, thisWeek.Item2, Period.Week).Sales;
        }

        public async Task<int> GetSalesThisMonth()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            List<Stat> stats = await this.GetStatsInRange(startDate, DateTime.Now);
            return this.Aggregate(stats, startDate, DateTime.Now, Period.Month).Sales;
        }

        public async Task<DateTime> GetLastUpdatedTime()
        {
            try
            {
                string query = string.Format("Select * from Stat order by strftime('%Y%m%d', datetime(\"{0}\",'localtime')) desc limit 0,1", "DateUpdated");

                List<Stat> latestStat = await this.Controller.SelectQueryAsync(query);

                if (latestStat == null || latestStat.Count == 0)
                {
                    return default(DateTime);
                }

                DateTime lastDateTime = latestStat.OrderByDescending(date => date.Modified).FirstOrDefault().Modified;
                return lastDateTime;
            }
            catch (Exception exception)
            {
                this._logger.Error(exception);
                throw;
            }
        }

        private StatsController Controller
        {
            get
            {
                this._statsController = this._statsController ?? new StatsController();
                return this._statsController;
            }
        }

        private DayOfWeek? StartDayOfWeek
        {
            get
            {
                this._startDayOfWeek = this._startDayOfWeek ?? Settings.Settings.Instance.StartOfWeek;
                return this._startDayOfWeek;
            }
        }

        public async Task<List<AggregatedStat>> GetAggregatedStats(Period period)
        {
            switch (period)
            {
                case Period.Day:
                    return await this.GetAggregatedDaysDataAsync();
                case Period.Week:
                    return await this.GetAggregatedWeeksDataAsync();
                case Period.Month:
                    return await this.GetAggregatedMonthsDataAsync();
            }

            return new List<AggregatedStat>();
        }

        private async Task<List<AggregatedStat>> GetAggregatedMonthsDataAsync()
        {
            const int monthsToShow = 12;
            List<AggregatedStat> aggregatedStats = new List<AggregatedStat>();
            for (int month = 0; month < monthsToShow; month++)
            {
                DateTime targetMonth = DateTime.Now.AddMonths(-1 * month);
                DateTime monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                DateTime monthFollowing = targetMonth.AddMonths(1);
                DateTime monthEnd = new DateTime(
                        targetMonth.Year,
                        targetMonth.Month,
                        new DateTime(monthFollowing.Year, monthFollowing.Month, 1).AddDays(-1).Day,
                        23,
                        59,
                        59);

                if (monthEnd > DateTime.Now)
                {
                    monthEnd = DateTime.Now;
                }

                List<Stat> stats = await this.GetStatsInRange(monthStart, monthEnd);
                if (stats != null && stats.Count > 0)
                {
                    aggregatedStats.Add(this.Aggregate(stats, monthStart.Date, monthEnd.Date, Period.Month));
                }
            }

            return aggregatedStats;
        }

        private async Task<List<AggregatedStat>> GetAggregatedDaysDataAsync()
        {
            try
            {
                DateTime lastUpdated =
                    await new RemoteSalesStatsService().GetLastUpdateDateTimeAsync(new CancellationTokenSource().Token);
                List<AggregatedStat> aggregatedStats = new List<AggregatedStat>();
                if (lastUpdated == default(DateTime))
                {
                    return aggregatedStats;
                }

                const int daysToShow = 6;
                for (int d = 0; d <= daysToShow; d++)
                {
                    DateTime queryDate = DateTime.Now.AddDays(-1 * (daysToShow - d));
                    List<Stat> stats = await this.GetStatsInRange(queryDate, queryDate);
                    if (stats != null && stats.Count > 0)
                    {
                        aggregatedStats.Add(this.Aggregate(stats, queryDate, queryDate, Period.Day));
                    }
                    else
                    {
                        aggregatedStats.Add(
                                new AggregatedStat
                                {
                                    Period = Period.Day,
                                    From = queryDate,
                                    To = queryDate,
                                    Sales = 0,
                                    Prospects = 0
                                });
                    }
                }

                return aggregatedStats.OrderByDescending(stat => stat.To).ToList();
            }
            catch (Exception exception)
            {
                this._logger.Error(exception);
                throw;
            }
        }

        private AggregatedStat Aggregate(List<Stat> stats, DateTime from, DateTime to, Period period)
        {
            AggregatedStat newAggregate = new AggregatedStat
            {
                From = from,
                To = to,
                Period = period
            };
            if (stats == null || stats.Count == 0)
            {
                newAggregate.Prospects = 0;
                newAggregate.Sales = 0;
            }
            else
            {
                newAggregate.Prospects = stats.Sum(stat => stat.Prospects);
                newAggregate.Sales = stats.Sum(stat => stat.Sales);
            }

            return newAggregate;
        }

        private async Task<List<AggregatedStat>> GetAggregatedWeeksDataAsync()
        {
            const int monthsToShow = 6;
            DateTime earliestMonth = DateTime.Now.AddMonths(1 - monthsToShow);
            DateTime startOfEarliestMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1);

            List<AggregatedStat> aggregatedStats = new List<AggregatedStat>();
            Tuple<DateTime, DateTime> week = this.GetWeekDayBelongsTo(startOfEarliestMonth);
            while (week.Item1 <= DateTime.Now)
            {
                List<Stat> stats = await this.GetStatsInRange(week.Item1, week.Item2);
                bool gotResults = stats != null && stats.Count > 0;
                if (gotResults || aggregatedStats.Count > 0)
                {
                    aggregatedStats.Add(this.Aggregate(stats, week.Item1, week.Item2, Period.Week));
                }

                week = this.GetWeekDayBelongsTo(week.Item2.AddDays(1));
            }

            return aggregatedStats.OrderByDescending(stat => stat.To).ToList();
        }

        /// <summary>
        /// Get stats using a date range
        /// </summary>
        /// <param name="from">Date from</param>
        /// <param name="to">Date to</param>
        /// DO NOT CHANGE THE CODE BELOW
        /// For some unknown reason C# does not allow us to manipulate params DateTime from & to in the method that's why we had to create these two variables.
        /// <returns>A List of stats</returns>
        private async Task<List<Stat>> GetStatsInRange(DateTime from, DateTime to)
        {
            try
            {
                DateTime fromLocal = from.ToMidnight();
                DateTime toLocal = to.ToEndOfDay();

                string sql = string.Format(
                        "Select * from Stat Where {0} between '{1}' and '{2}'",
                        CriteriaBuilder.GetFormattedDateField("Date"),
                        CriteriaBuilder.GetDateValueFormatedForQuery(fromLocal, true),
                        CriteriaBuilder.GetDateValueFormatedForQuery(toLocal, true));

                this._logger.Info(sql);
                var stats = await this.Controller.SelectQueryAsync(sql);
                return stats;
            }
            catch (Exception exception)
            {
                this._logger.Error(exception);
                throw;
            }
        }
    }
}