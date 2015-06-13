using System;
using Coditate.Common.IO;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Utility methods for formatting display messages.
    /// </summary>
    public static class FormatUtils
    {
        /// <summary>
        /// Date format string: MMM d \"at\" h:mm tt
        /// </summary>
        public const string CurrentYearDateTimeFormat = "MMM d \"at\" h:mm tt";

        /// <summary>
        /// Date format string: MMM d \"'\"yy \"at\" h:mm tt
        /// </summary>
        public const string FullDateTimeFormat = "MMM d \"'\"yy \"at\" h:mm tt";

        /// <summary>
        /// Formats a byte count as megabytes.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <param name="includeLabel">if set to <c>true</c> include the trailing "MB" label.</param>
        /// <returns></returns>
        public static string BytesAsMegaBytes(long byteCount, bool includeLabel)
        {
            string formatted = ((double) byteCount/IOUtils.BytesPerMegabyte).ToString("#,0.0");
            if (includeLabel)
            {
                formatted += " MB";
            }
            return formatted;
        }

        /// <summary>
        /// Formats date for a particular timezone following
        /// the adjustment rules described for <see cref="DateUtils.Local"/>.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="utcOffset">The <em>current</em> UTC offset.</param>
        /// <param name="format">The format string.</param>
        /// <returns></returns>
        /// <see cref="DateUtils.Local"/>
        public static string DateAsLocal(DateTime date, TimeSpan utcOffset, string format)
        {
            return DateUtils.Local(date, utcOffset).ToString(format);
        }

        /// <summary>
        /// Formats date for a particular timezone following
        /// the adjustment rules described for <see cref="DateUtils.Local"/>.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="utcOffset">The <em>current</em> UTC offset.</param>
        /// <returns></returns>
        /// <see cref="DateUtils.Local"/>
        /// <remarks>
        /// A format string is provided automatically based on the distance of the date from the current date and time. 
        /// </remarks>
        public static string DateAsLocal(DateTime date, TimeSpan utcOffset)
        {
            Arg.CheckCondition("date", date.Kind == DateTimeKind.Utc, "Not a Utc DateTime");

            DateTime now = DateTime.UtcNow;

            string format = FullDateTimeFormat;
            if (now.Year == date.Year)
            {
                format = CurrentYearDateTimeFormat;
            }
            if (now.Year == date.Year && now.Month == date.Month)
            {
                format = CurrentYearDateTimeFormat;
            }
            if (now.Year == date.Year && now.Month == date.Month && now.Day == date.Day)
            {
                format = "Today \"at\" h:mm tt";
            }

            return DateUtils.Local(date, utcOffset).ToString(format);
        }

        /// <summary>
        /// Formats a zipcode for display
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        public static string ZipCodeForDisplay(string zipCode)
        {
            if (zipCode == null)
            {
                return null;
            }
            string formatted = zipCode.Replace("-", "");
            if (formatted.Length > 5)
            {
                formatted = formatted.Substring(0, 5) + '-' + formatted.Substring(5);
            }
            return formatted;
        }

        /// <summary>
        /// Provides a relative date description (5 min ago, 1 day ago, etc.) for a particular timezone following
        /// the adjustment rules described for <see cref="DateUtils.Local"/>.
        /// </summary>
        /// <param name="utcThen">The date.</param>
        /// <param name="utcOffset">The <em>current</em> UTC offset.</param>
        /// <returns></returns>
        /// <see cref="DateUtils.Local"/>
        public static string DateToLocalRelative(DateTime utcThen, TimeSpan utcOffset)
        {
            return DateToLocalRelative(utcThen, DateTime.UtcNow, utcOffset);
        }

        internal static string DateToLocalRelative(DateTime utcThen, DateTime utcNow, TimeSpan utcOffset)
        {
            Arg.CheckCondition("utcThen", utcThen.Kind == DateTimeKind.Utc, "Not a Utc DateTime");
            Arg.CheckCondition("utcNow", utcNow.Kind == DateTimeKind.Utc, "Not a Utc DateTime");

            TimeSpan difference = utcNow - utcThen;
            var totalDays = (long) difference.TotalDays;
            var totalHours = (long) difference.TotalHours;
            if (difference.Ticks > TimeSpan.TicksPerDay*365)
            {
                return DateAsLocal(utcThen, utcOffset, FullDateTimeFormat);
            }
            if (difference.Ticks > TimeSpan.TicksPerDay*7)
            {
                return DateAsLocal(utcThen, utcOffset, CurrentYearDateTimeFormat);
            }
            if (difference.Ticks > TimeSpan.TicksPerDay)
            {
                return string.Format("{0:0} day{1} ago", totalDays, totalDays > 1 ? "s" : "");
            }
            if (difference.Ticks > TimeSpan.TicksPerHour)
            {
                return string.Format("{0:0} hour{1} ago", totalHours, totalHours > 1 ? "s" : "");
            }
            return difference.TotalMinutes.ToString("0") + " min ago";
        }
    }
}