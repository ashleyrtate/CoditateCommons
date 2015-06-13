using System;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Enumeration of ways to round <see cref="DateTime"/> objects.
    /// </summary>
    public enum DateRounding
    {
        /// <summary>
        /// Round to the second.
        /// </summary>
        Second,
        /// <summary>
        /// Round to the minute.
        /// </summary>
        Minute,
        /// <summary>
        /// 
        /// </summary>
        Hour,
        /// <summary>
        /// 
        /// </summary>
        Day,
        /// <summary>
        /// Round to the month.
        /// </summary>
        Month,
        /// <summary>
        /// 
        /// </summary>
        Year
    }

    /// <summary>
    /// Utility methods for working with dates and times.
    /// </summary>
    public static class DateUtils
    {
        /// <summary>
        /// Milliseconds per second. Value is 1000.
        /// </summary>
        public const int MillisecondsPerSecond = 1000;

        /// <summary>
        /// Seconds per minute. Value is 60.
        /// </summary>
        public const int SecondsPerMinute = 60;

        /// <summary>
        /// Adjusts UTC dates for a particular timezone. Non-UTC dates are 
        /// returned without adjustment.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="utcOffset">The <em>current</em> UTC offset.</param>
        /// <returns></returns>
        /// <remarks>
        /// The returned time is adjusted for DST mismatches. If DST is currently in effect 
        /// but wasn't in effect for the date being formatted (or vice versa), then
        /// applying the current UTC offset as-is would result in an incorrect time.
        /// </remarks>
        public static DateTime Local(DateTime date, TimeSpan utcOffset)
        {
            DateTime adjusted = date;
            if (date.Kind == DateTimeKind.Utc)
            {
                adjusted = new DateTime(date.Ticks, DateTimeKind.Local).Add(utcOffset);
                adjusted = AdjustForDstMismatch(adjusted);
            }
            return adjusted;
        }

        private static DateTime AdjustForDstMismatch(DateTime formatTime)
        {
            TimeSpan ts1 = TimeZone.CurrentTimeZone.GetUtcOffset(formatTime);
            TimeSpan ts2 = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            TimeSpan adjustment = ts2 - ts1;
            return formatTime - adjustment;
        }

        /// <summary>
        /// Rounds a <see cref="DateTime"/> to the specified precision.
        /// </summary>
        /// <param name="dt">The datetime to round.</param>
        /// <param name="rounding">The rounding type.</param>
        /// <returns></returns>
        public static DateTime Round(DateTime dt, DateRounding rounding)
        {
            int year = dt.Year;
            int month = dt.Month;
            int day = dt.Day;
            int hour = dt.Hour;
            int minute = dt.Minute;
            int second = dt.Second;
            switch (rounding)
            {
                case DateRounding.Year:
                    month = 1;
                    day = 1;
                    hour = 0;
                    minute = 0;
                    second = 0;
                    break;
                case DateRounding.Month:
                    day = 1;
                    hour = 0;
                    minute = 0;
                    second = 0;
                    break;
                case DateRounding.Day:
                    hour = 0;
                    minute = 0;
                    second = 0;
                    break;
                case DateRounding.Hour:
                    minute = 0;
                    second = 0;
                    break;
                case DateRounding.Minute:
                    second = 0;
                    break;
                case DateRounding.Second:
                    // nothing to do
                    break;
                default:
                    throw new InvalidOperationException("Unsupported rounding value: " + rounding);
            }
            return new DateTime(year, month, day, hour, minute, second, dt.Kind);
        }

        /// <summary>
        /// Rounds the specified TimeStamp.
        /// </summary>
        /// <param name="ts">The TimeStamp.</param>
        /// <param name="rounding">The rounding type.</param>
        /// <returns></returns>
        public static TimeSpan Round(TimeSpan ts, DateRounding rounding)
        {
            int days = ts.Days;
            int hours = ts.Hours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            switch (rounding)
            {
                case DateRounding.Minute:
                    minutes += (int)Math.Round(((double)seconds / SecondsPerMinute), MidpointRounding.AwayFromZero);
                    seconds = 0;
                    break;
                case DateRounding.Second:
                    seconds +=
                        (int)
                        Math.Round(((double)ts.Milliseconds/MillisecondsPerSecond), MidpointRounding.AwayFromZero);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported rounding value: " + rounding);
            }
            return new TimeSpan(days, hours, minutes, seconds);
        }
    }
}