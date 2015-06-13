using System.Collections.Generic;
using Coditate.Common.Util;
using NUnit.Framework;

namespace Coditate.TestSupport
{
    [TestFixture]
    public class PropertyMatcherTest
    {
        private class A
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void AreEqual()
        {
            A a1 = new A
                {
                    IntValue = RandomData.Int(),
                    StringValue = RandomData.AsciiString(100)
                };
            A a2 = new A
                {
                    IntValue = a1.IntValue,
                    StringValue = a1.StringValue
                };

            Assert.IsTrue(PropertyMatcher.AreEqual(a1, a2).Equal);

            a2.IntValue = RandomData.Int();
            a2.StringValue = RandomData.AlphaNumericString(100, true);

            Assert.IsFalse(PropertyMatcher.AreEqual(a1, a2).Equal);

            a2.IntValue = a1.IntValue;
            a2.StringValue = null;

            Assert.IsFalse(PropertyMatcher.AreEqual(a1, a2).Equal);

            a1.StringValue = null;

            Assert.IsTrue(PropertyMatcher.AreEqual(a1, a2).Equal);

            a2.StringValue = RandomData.AsciiString(100);

            Assert.IsFalse(PropertyMatcher.AreEqual(a1, a2).Equal);
        }

        [Test]
        public void AreEqualEnumerable()
        {
            List<A> first = new List<A> {new A()};
            List<A> second = new List<A> {new A()};

            Assert.IsTrue(PropertyMatcher.AreEqual(first, second).Equal);
        }

        [Test]
        public void AreEqualEnumerable_Excluded()
        {
            List<A> first = new List<A> { new A{IntValue = RandomData.Int(), StringValue = "abc"} };
            List<A> second = new List<A> { new A { IntValue = RandomData.Int(), StringValue = "abc" } };

            Assert.IsTrue(PropertyMatcher.AreEqual(first, second, "IntValue").Equal);
        }

        [Test]
        public void AreEqual_Excluded()
        {
            A a1 = new A
                {
                    IntValue = RandomData.Int(),
                    StringValue = RandomData.AsciiString(100)
                };
            A a2 = new A
                {
                    IntValue = RandomData.Int(),
                    StringValue = a1.StringValue
                };

            Assert.IsFalse(PropertyMatcher.AreEqual(a1, a2).Equal);
            Assert.IsTrue(PropertyMatcher.AreEqual(a1, a2, "IntValue").Equal);
        }

        [Test]
        public void AreEqual_NullObjects()
        {
            A a = new A();

            Assert.IsFalse(PropertyMatcher.AreEqual(a, null).Equal);
            Assert.IsFalse(PropertyMatcher.AreEqual(null, a).Equal);
            Assert.IsTrue(PropertyMatcher.AreEqual(null, null).Equal);
        }
    }
}