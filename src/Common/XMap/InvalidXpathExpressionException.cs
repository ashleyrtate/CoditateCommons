using System;

namespace Coditate.Common.XMap
{
    /// <summary>
    /// Thrown by <see cref="XMap"/> to indicate use of an invalid Xpath expression.
    /// </summary>
    /// <remarks>
    /// The expression may be invalid because it is malformed or because it is partially,
    /// but not fully resolvable.
    /// 
    /// <para>A partially resolvable expression is one where the root part of the
    /// expression resolves to an node in the Xml document but the full expression 
    /// does not resolve to any node.</para>
    /// </remarks>
    public class InvalidXpathExpressionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXpathExpressionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="xpath">The node path.</param>
        public InvalidXpathExpressionException(string message, string xpath)
            : this(message, xpath, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXpathExpressionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="xpath">The node path.</param>
        /// <param name="inner">The inner exception.</param>
        public InvalidXpathExpressionException(string message, string xpath, Exception inner) : base(message, inner)
        {
            XPath = xpath;
        }

        /// <summary>
        /// Gets or sets the node path.
        /// </summary>
        /// <value>The node path.</value>
        public string XPath { get; private set; }
    }
}