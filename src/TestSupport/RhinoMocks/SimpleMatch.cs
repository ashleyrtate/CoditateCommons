using System.Collections;
using Coditate.Common.Util;
using Rhino.Mocks.Constraints;

namespace Coditate.TestSupport.RhinoMocks
{
    /// <summary>
    /// Constraint which expects that two objects should have equivalent simple properties.
    /// </summary>
    /// <remarks>
    /// If the expected object is enumerable, then the evaulated object is also expected to be
    /// enumerable and contain matching component objects. This is to aid in checking methods with 
    /// <c>params</c> arguments.
    /// </remarks>
    public class SimpleMatch : AbstractConstraint
    {
        private readonly object expected;
        private string[] excludedProperties;
        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToStringEqual"/> class.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        public SimpleMatch(object expected) : this(expected, new string[] {})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMatch"/> class.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        /// <param name="excludedProperties">The excluded properties.</param>
        public SimpleMatch(object expected, params string[] excludedProperties)
        {
            Arg.CheckNull("expected", expected);
            Arg.CheckNull("excludedProperties", excludedProperties);

            this.expected = expected;
            this.excludedProperties = excludedProperties;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public override string Message
        {
            get { return "[[equal to '" + expected + "': " + message + "]]"; }
        }

        /// <summary>
        /// Evals the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public override bool Eval(object obj)
        {
            var matcher = new PropertyMatcher();
            PropertyMatcher.MatchResult result;
            if (expected is IEnumerable)
            {
                if (obj != null && !(obj is IEnumerable))
                {
                    result = new PropertyMatcher.MatchResult(false,
                                                             "The first object is IEnumerable but the second is " +
                                                             obj.GetType());
                }
                else
                {
                    result = PropertyMatcher.AreEqual(expected as IEnumerable, obj as IEnumerable, excludedProperties);
                }
            }
            else
            {
                result =
                    PropertyMatcher.AreEqual(expected, obj, excludedProperties);
            }
            string index = "";
            if (result.Index >= 0)
            {
                index = " (index=" + result.Index + ")";
            }

            message = result.Message + index;
            return result.Equal;
        }
    }
}