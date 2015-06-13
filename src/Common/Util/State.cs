using System;
using System.Reflection;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Utility class for performing general assertions about object or program state.
    /// </summary>
    public static class State
    {
        /// <summary>
        /// Checks whether a property value is null.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="source">The object from which to read the property value.</param>
        /// <exception cref="InvalidOperationException">If the property value is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the named property is undefined or can't be read</exception>
        public static void CheckPropertyNull(string name, object source)
        {
            PropertyInfo propertyInfo = source.GetType().GetProperty(name,
                                                                     BindingFlags.Public |
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.Static |
                                                                     BindingFlags.FlattenHierarchy);

            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(name, "Not a defined property");
            }

            object value;
            try
            {
                value = propertyInfo.GetValue(source, null);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("Unable to read property named: " + name, ex);
            }

            if (value == null)
            {
                string message = string.Format("Required property '{0}' is null", name);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Checks a condition and throws an <see cref="InvalidOperationException"/> if the condition is false.
        /// </summary>
        /// <param name="condition">The condition that should be true.</param>
        /// <param name="message">The exception message if condition is false</param>
        /// <exception cref="InvalidOperationException">If condition is false</exception>
        public static void CheckFalse(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Checks a condition and throws an <see cref="InvalidOperationException"/> if the condition is true.
        /// </summary>
        /// <param name="condition">The condition that should be false.</param>
        /// <param name="message">The exception message if condition is true</param>
        /// <exception cref="InvalidOperationException">If condition is true</exception>
        public static void CheckTrue(bool condition, string message)
        {
            CheckFalse(!condition, message);
        }
    }
}