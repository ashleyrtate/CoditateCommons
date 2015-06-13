using System;
using System.Collections;
using System.Text;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Utility methods for performing common string operations.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Indicates whether the string is null or empty after optionally trimming whitespace.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="trim">if set to <c>true</c> trim non-null strings before checking them.</param>
        /// <returns>
        /// 	<c>true</c> if the string is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string value, bool trim)
        {
            string test = value;
            if (value != null && trim)
            {
                test = value.Trim();
            }
            return string.IsNullOrEmpty(test);
        }

        /// <summary>
        /// Trims the string if it is not null.
        /// </summary>
        /// <param name="value">The string to trim.</param>
        /// <returns>The trimmed string or null if the original string was null</returns>
        public static string TrimEx(this string value)
        {
            if (value == null)
            {
                return value;
            }
            return value.Trim();
        }

        /// <summary>
        /// Truncates the specified string.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="maxLength">Max length of truncated string.</param>
        /// <returns></returns>
        public static string Truncate(string value, int maxLength)
        {
            return Truncate(value, maxLength, false);
        }

        /// <summary>
        /// Truncates the specified string and optionally adds a trailing ellipsis.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="maxLength">Max length of truncated string.</param>
        /// <param name="autoEllipsis">if set to <c>true</c> replace last 3 characters of truncated text with an ellipsis.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">If maxLength param is less than zero</exception>
        public static string Truncate(string value, int maxLength, bool autoEllipsis)
        {
            Arg.CheckInRange("maxLength", maxLength, 0, int.MaxValue);

            if (value == null || value.Length <= maxLength)
            {
                return value;
            }
            var truncated = new StringBuilder(maxLength);
            if (autoEllipsis)
            {
                int adjustedMaxLength = Math.Max(0, maxLength - 3);
                truncated.Append(value.Substring(0, adjustedMaxLength));
                if (truncated.Length > 0)
                {
                    truncated.Append("...");
                }
            }
            else
            {
                truncated.Append(value.Substring(0, maxLength));
            }
            return truncated.ToString();
        }

        /// <summary>
        /// Replaces all occurrences of specified characters with a new character.
        /// </summary>
        /// <param name="oldChars">The old characters.</param>
        /// <param name="newChar">The new chararacter.</param>
        /// <param name="s">The string to modified.</param>
        /// <returns></returns>
        public static string ReplaceChars(char[] oldChars, char newChar, string s)
        {
            var b = new StringBuilder(s);
            foreach (char c in oldChars)
            {
                b.Replace(c, newChar);
            }
            return b.ToString();
        }

        /// <summary>
        /// Determines whether the specified string contains any of a list of strings.
        /// </summary>
        /// <param name="check">The string to check.</param>
        /// <param name="values">The values to check for.</param>
        /// <returns>
        /// 	<c>true</c> if any values are contained in the check string; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string check, string[] values)
        {
            Arg.CheckNull("check", check);
            Arg.CheckNull("values", values);

            foreach (string s in values)
            {
                if (check.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified string ends with any of a list of strings.
        /// </summary>
        /// <param name="check">The string to check.</param>
        /// <param name="values">The values to check for.</param>
        /// <param name="comparisonType">The comparison type.</param>
        /// <returns></returns>
        public static bool EndsWith(string check, string[] values, StringComparison comparisonType)
        {
            Arg.CheckNull("check", check);
            Arg.CheckNull("values", values);

            foreach (string s in values)
            {
                if (check.EndsWith(s, comparisonType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Variation of <see cref="string.Join(string, string[])"/> that accepts
        /// any enumerable list of objects and joins them with a separating delimiter.
        /// </summary>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="values">The list of values to join.</param>
        /// <returns></returns>
        public static string Join(string delimiter, IEnumerable values)
        {
            Arg.CheckNullOrEmpty("delimiter", delimiter);
            Arg.CheckNull("values", values);

            var sb = new StringBuilder();
            IEnumerator e = values.GetEnumerator();
            bool hasValue = e.MoveNext();
            if (!hasValue)
            {
                return sb.ToString();
            }
            do
            {
                sb.Append(e.Current);
                hasValue = e.MoveNext();
                if (hasValue)
                {
                    sb.Append(delimiter);
                }
            } while (hasValue);
            return sb.ToString();
        }
    }
}