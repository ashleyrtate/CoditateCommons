using System;
using NUnit.Framework;
using Coditate.Common.Util;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Unit tests for <see cref="Arg"/> class.
    /// </summary>
    [TestFixture]
    public class ArgTest
    {
        [Test]
        [ExpectedException(typeof (ArgumentNullException),
            ExpectedMessage = @"Value cannot be null.
Parameter name: propertyName")]
        public void CheckNull()
        {
            Arg.CheckNull("propertyName", null);
        }
    }
}