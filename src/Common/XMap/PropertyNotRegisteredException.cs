using System;

namespace Coditate.Common.XMap
{
    /// <summary>
    /// Thrown by <see cref="XMap"/> to indicate use of an unregistered property.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PropertyNotRegisteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXpathExpressionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="property">The property.</param>
        public PropertyNotRegisteredException(string message, string property)
            : this(message, property, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXpathExpressionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="property">The property.</param>
        /// <param name="inner">The inner exception.</param>
        public PropertyNotRegisteredException(string message, string property, Exception inner)
            : base(message, inner)
        {
            Property = property;
        }

        /// <summary>
        /// Gets or sets the node path.
        /// </summary>
        /// <value>The node path.</value>
        public string Property { get; private set; }
    }
}