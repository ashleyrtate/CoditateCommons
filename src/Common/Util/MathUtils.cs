using System;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Utility methods for mathematical operations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Calculates percent as an integer, given a fractional numerator and denominator.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <param name="constrain">if set to <c>true</c> constrain returned value to a minimum of zero and max of 100.</param>
        /// <remarks>
        /// If denominator is zero, then 100 is returned.
        /// </remarks>
        /// <returns></returns>
        public static int ToPercent(double numerator, double denominator, bool constrain)
        {
            if (denominator == 0)
            {
                return 100;
            }
            double decimalValue = numerator/denominator;
            var percent = (int) ((decimalValue)*100);

            if (constrain)
            {
                percent = Constrain(percent, 0, 100);
            }

            return percent;
        }

        /// <summary>
        /// Constrains a value between a minimum and maximum.
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The min value to return.</param>
        /// <param name="max">The max value to return.</param>
        /// <returns>
        /// The provided value or the closest value within the requested range.
        /// </returns>
        /// <remarks></remarks>
        public static int Constrain(int value, int min, int max)
        {
            value = Math.Min(value, max);
            value = Math.Max(value, min);
            return value;
        }

        /// <summary>
        /// Constrains a value between a minimum and maximum.
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The min value to return.</param>
        /// <param name="max">The max value to return.</param>
        /// <returns>
        /// The provided value or the closest value within the requested range.
        /// </returns>
        /// <remarks></remarks>
        public static double Constrain(double value, double min, double max)
        {
            value = Math.Min(value, max);
            value = Math.Max(value, min);
            return value;
        }
    }
}