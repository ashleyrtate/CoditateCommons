using System.Collections;
using System.Linq;
using System.Reflection;
using Coditate.Common.Util;

namespace Coditate.TestSupport
{
    /// <summary>
    /// Performs property value comparisions between objects.
    /// </summary>
    public class PropertyMatcher
    {
        /// <summary>
        /// Checks whether the component elements of the two enumerable objects match.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <param name="excludedProperties">The properties to exclude from comparison.</param>
        /// <returns></returns>
        public static MatchResult AreEqual(IEnumerable first, IEnumerable second, params string[] excludedProperties)
        {
            MatchResult result = CompareNullState(first, second);
            if (result != null)
            {
                return result;
            }

            int index = 0;
            IEnumerator secondEnumerator = second.GetEnumerator();
            foreach (object firstItem in first)
            {
                if (!secondEnumerator.MoveNext())
                {
                    return new MatchResult(false, "The second list has fewer items than the first");
                }
                result = AreEqual(firstItem, secondEnumerator.Current, excludedProperties);
                if (!result.Equal)
                {
                    result.Index = index;
                    return result;
                }
                index++;
            }
            if (secondEnumerator.MoveNext())
            {
                return new MatchResult(false, "The first list has fewer items than the second");
            }

            return new MatchResult(true, "Objects are equal");
        }

        /// <summary>
        /// Checks whether the property values of the first object match those of
        /// the second.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <param name="excludedProperties">The properties to exclude from comparison.</param>
        /// <returns></returns>
        /// <remarks>
        /// Properties are excluded from comparision if rejected by <see cref="PropertyUtils.IsAcceptableProperty"/>.
        /// This method supports comparision of properties from different types of objects.
        /// </remarks>
        public static MatchResult AreEqual(object first, object second, params string[] excludedProperties)
        {
            MatchResult result = CompareNullState(first, second);
            if (result != null)
            {
                return result;
            }

            foreach (PropertyInfo pFirst in PropertyUtils.GetProperties(first.GetType(), true, true))
            {
                if (excludedProperties.Contains(pFirst.Name))
                {
                    continue;
                }

                PropertyInfo pSecond = PropertyUtils.FindMatching(pFirst, second.GetType());
                if (pSecond == null)
                {
                    return new MatchResult(false,
                                           string.Format("No match for property '{0}' was found on second object",
                                                         pFirst.Name));
                }

                object firstVal = pFirst.GetValue(first, null);
                object secondVal = pSecond.GetValue(second, null);
                if (!Equals(firstVal, secondVal))
                {
                    return new MatchResult(false,
                                           string.Format(
                                               "Values for property '{0}' are not equal: first = [{1}], second = [{2}]",
                                               pFirst.Name, firstVal, secondVal));
                }
            }
            return new MatchResult(true, "Objects are equal");
        }

        private static MatchResult CompareNullState(object first, object second)
        {
            if (first == null && second == null)
            {
                return new MatchResult(true, "Both objects are null");
            }
            if (first == null ^ second == null)
            {
                return new MatchResult(false, "One object is null but the other is not");
            }
            return null;
        }

        /// <summary>
        /// Holds the results of a comparision between two objects.
        /// </summary>
        public class MatchResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MatchResult"/> class.
            /// </summary>
            /// <param name="equal">if set to <c>true</c> [equal].</param>
            /// <param name="message">The message.</param>
            public MatchResult(bool equal, string message)
            {
                Equal = equal;
                Message = message;
                Index = -1;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the compared objects were equal.
            /// </summary>
            /// <value><c>true</c> if equal; otherwise, <c>false</c>.</value>
            public bool Equal { get; private set; }

            /// <summary>
            /// Gets or sets the detailed comparision result message.
            /// </summary>
            /// <value>The result message.</value>
            public string Message { get; private set; }

            /// <summary>
            /// Gets or sets the object index (used only when comparing lists).
            /// </summary>
            /// <value>The index.</value>
            /// <remarks>A negative index value indicates that the objects were 
            /// not lists or were equivalent lists</remarks>
            public int Index { get; set; }
        }
    }
}