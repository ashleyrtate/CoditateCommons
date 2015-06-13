using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Coditate.TestSupport
{
    /// <summary>
    /// Assert operations that go beyond those supported in NUnit.
    /// </summary>
    public static class AssertEx
    {
        /// <summary>
        /// Asserts that the contents of two streams are exactly equal.
        /// </summary>
        /// <param name="first">The first stream.</param>
        /// <param name="second">The second stream.</param>
        public static void AreEqual(Stream first, Stream second)
        {
            int index = 0;
            int a, b;
            do
            {
                a = first.ReadByte();
                b = second.ReadByte();

                Assert.AreEqual(a, b, "index = " + index);
                index++;
            } while (a != -1 && b != -1);
        }

        /// <summary>
        /// Asserts that the contents of two readers are exactly equal.
        /// </summary>
        /// <param name="first">The first reader.</param>
        /// <param name="second">The second reader.</param>
        public static void AreEqual(TextReader first, TextReader second)
        {
            int index = 0;
            string a, b;
            do
            {
                a = first.ReadLine();
                b = second.ReadLine();

                Assert.AreEqual(a, b, "line = " + index);
                index++;
            } while (a != null && b != null);
        }

        /// <summary>
        /// Asserts that a string contains a particular substring.
        /// </summary>
        /// <param name="s">The string to check.</param>
        /// <param name="substring">The substring.</param>
        public static void Contains(string s, string substring)
        {
            Assert.That(s, new SubstringConstraint(substring));
        }
    }
}