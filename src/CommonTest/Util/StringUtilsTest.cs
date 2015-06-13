using System;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class StringUtilsTest
    {
        [Test]
        public void Contains()
        {
            string check = "";
            string[] values = {};

            Assert.IsFalse(StringUtils.Contains(check, values));

            check = "123abc123";
            values = new[] {"abc", "xyz"};

            Assert.IsTrue(StringUtils.Contains(check, values));
        }

        [Test]
        public void EndsWith()
        {
            string check = "";
            string[] values = {};

            Assert.IsFalse(StringUtils.EndsWith(check, values, StringComparison.InvariantCultureIgnoreCase));

            check = "123abc";
            values = new[] {"abc", "xyz"};

            Assert.IsTrue(StringUtils.EndsWith(check, values, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void Truncate()
        {
            int length = 10;
            string value = RandomData.AlphaNumericString(length, false);

            string zero = StringUtils.Truncate(value, 0);
            string shorter = StringUtils.Truncate(value, length - 1);
            string same = StringUtils.Truncate(value, length);
            string longer = StringUtils.Truncate(value, length + 1);

            Assert.AreEqual("", zero);
            Assert.AreEqual(shorter.Length, length - 1);
            Assert.AreEqual(value, same);
            Assert.AreEqual(value, longer);

            Assert.IsNull(StringUtils.Truncate(null, length));
            Assert.AreEqual("", StringUtils.Truncate("", length));
        }

        [Test]
        public void TruncateWithEllipsis()
        {
            int length = 10;
            string value = RandomData.AlphaNumericString(length, false);

            string zero = StringUtils.Truncate(value, 0, true);
            string shorter = StringUtils.Truncate(value, length - 1, true);
            string same = StringUtils.Truncate(value, length, true);
            string longer = StringUtils.Truncate(value, length + 1, true);

            Assert.AreEqual("", zero);
            Assert.AreEqual(shorter.Length, length - 1);
            Assert.IsTrue(shorter.EndsWith("..."));
            Assert.AreEqual(value, same);
            Assert.AreEqual(value, longer);

            Assert.IsNull(StringUtils.Truncate(null, length, true));
            Assert.AreEqual("", StringUtils.Truncate("", length, true));
        }

        [Test]
        public void JoinObjects()
        {
            var objects = new object[] { 1, "abc", TimeSpan.FromDays(1) };
            string expected = "1--abc--1.00:00:00";
            string actual = StringUtils.Join("--", objects);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void JoinObjectsEmpty()
        {
            var objects = new object[] { };
            string expected = "";
            string actual = StringUtils.Join("*", objects);

            Assert.AreEqual(expected, actual);
        }
    }
}