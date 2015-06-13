using System;
using Rhino.Mocks.Constraints;

namespace Coditate.TestSupport.RhinoMocks
{
    /// <summary>
    /// Constraint which expects that two objects should evaluate to the same string (case insensitive).
    /// </summary>
    public class ToStringEqual : AbstractConstraint
    {
        private readonly object expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToStringEqual"/> class.
        /// </summary>
        /// <param name="expected">The expected object.</param>
        public ToStringEqual(object expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public override string Message
        {
            get { return "equal to string '" + expected + "'"; }
        }

        /// <summary>
        /// Evals the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public override bool Eval(object obj)
        {
            return
                string.Equals(expected.ToString(), obj.ToString(),
                              StringComparison.InvariantCultureIgnoreCase);
        }
    }
}