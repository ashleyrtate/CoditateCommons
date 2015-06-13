using System;
using System.Collections.Generic;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Provides functions for checking method arguments.
    /// </summary>
    /// <remarks>
    /// Each CheckXxy() method will throw an appropriate exception with an informative, generic
    /// error message if the check condition is not met.
    /// </remarks>
    public static class Arg
    {
        /// <summary>
        /// Checks if the argument is assignable to (inherits from)
        /// a specified type.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="expectedType">Expected type or parent type of the argument value</param>
        /// <exception cref="ArgumentException">If value is not an instance of expectedType</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckIsType(string name, object value, Type expectedType)
        {
            CheckNull(name, value);

            CheckIsAssignableTo(name, value.GetType(), expectedType);
        }

        /// <summary>
        /// Checks if a Type argument is assignable to (inherits from)
        /// a specified type.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="expectedType">Expected type or parent type of the argument Type</param>
        /// <exception cref="ArgumentException">If value is not assignable to expectedType</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckIsAssignableTo(string name, Type value, Type expectedType)
        {
            CheckNull(name, value);

            if (!expectedType.IsAssignableFrom(value))
            {
                string message =
                    string.Format("Expected type of [{0}] but was [{1}].", expectedType.FullName,
                                  value.FullName);
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Checks if the argument value is in the specified range.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="min">The bottom of the range (inclusive).</param>
        /// <param name="max">The top of the range (inclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range</exception>
        public static void CheckInRange(string name, double value, double min, double max)
        {
            if (value < min || value > max)
            {
                string message =
                    string.Format("Expected value between {0} and {1} (inclusive) but was {2}.", min,
                                  max, value);
                throw new ArgumentOutOfRangeException(name, message);
            }
        }

        /// <summary>
        /// Checks if the argument is null.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="value">The argument value.</param>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckNull(string name, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Checks if a string argument is null or empty.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <exception cref="ArgumentOutOfRangeException">If value is empty</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckNullOrEmpty(string name, string value)
        {
            CheckNull(name, value);

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException(name, "An empty string is not allowed.");
            }
        }

        /// <summary>
        /// Checks if an array argument is of at least a certain length.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="minLength">Minimum expected length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If value.Length is less than minLength</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckMinLength(string name, object[] value, int minLength)
        {
            CheckNull(name, value);

            CheckMinLengthImpl(name, value.Length, minLength);
        }

        /// <summary>
        /// Checks if an array argument is of at least a certain length.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="minLength">Minimum expected length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If value.Length is less than minLength</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckMinLength(string name, byte[] value, int minLength)
        {
            CheckNull(name, value);

            CheckMinLengthImpl(name, value.Length, minLength);
        }

        /// <summary>
        /// Checks if an ICollection argument is of at least a certain length.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <param name="minLength">Minimum expected length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If value.Length is less than minLength</exception>
        /// <exception cref="ArgumentNullException">If value is null</exception>
        public static void CheckMinLength<T>(string name, ICollection<T> value, int minLength)
        {
            CheckNull(name, value);

            CheckMinLengthImpl(name, value.Count, minLength);
        }

        /// <summary>
        /// Checks if an expected true boolean value is actually false.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="trueCondition">The condition that should be true.</param>
        /// <param name="message">The exception message if trueCondition is false.</param>
        /// <exception cref="ArgumentException">If trueCondition is false</exception>
        public static void CheckCondition(string name, bool trueCondition, string message)
        {
            if (!trueCondition)
            {
                throw new ArgumentException(message, name);
            }
        }

        private static void CheckMinLengthImpl(string name, int length, int minLength)
        {
            if (length < minLength)
            {
                string message = string.Format("Length is less than {0}.", minLength);
                throw new ArgumentOutOfRangeException(name, message);
            }
        }
    }
}