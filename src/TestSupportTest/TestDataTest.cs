using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Coditate.TestSupport
{
    internal enum TestEnum
    {
        First,
        Second,
        Third
    }

    [TestFixture]
    public class TestDataTest
    {
        private static readonly Random Rand = new Random();

        [Test]
        public void RandomAsciiString()
        {
            int length = Rand.Next(0, 10000);
            string randomString = TestData.RandomAsciiString(length);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(c >= 0x20 || c <= 0x7E, c.ToString());
            }
        }

        [Test]
        public void RandomAlphaNumericString()
        {
            int length = Rand.Next(0, 10000);
            string randomString = TestData.RandomAlphaNumericString(length);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(c == ' ' || char.IsLetterOrDigit(c), c.ToString());
            }
        }

        [Test]
        public void RandomAlphaNumericString_NoSpaces()
        {
            int length = Rand.Next(0, 10000);
            string randomString = TestData.RandomAlphaNumericString(length, false);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(char.IsLetterOrDigit(c), c.ToString());
            }
        }

        [Test]
        public void RandomAlphaNumericString_NoConsecutiveSpaces()
        {
            int length = Rand.Next(0, 10000);
            string randomString = TestData.RandomAlphaNumericString(length, true);

            Assert.AreEqual(length, randomString.Length);
            char lastC = '0';
            foreach (char c in randomString)
            {
                if (c == ' ')
                {
                    Assert.AreNotEqual(lastC, c);
                }
                lastC = c;
            }
        }

        [Test]
        public void RandomBool()
        {
            double runs = 100;
            int count = (int) runs/2;
            for (int k = 0; k < runs; k++)
            {
                if (TestData.RandomBool())
                {
                    count++;
                }
                else
                {
                    count--;
                }
            }

            // verify that true and false are each returned at least 10% of the time
            Assert.Greater(count, runs*.1);
            Assert.Less(count, runs*.9);
        }

        [Test]
        public void RandomListValue()
        {
            int count = 3;
            List<object> objects = new List<object>();
            int[] returns = new int[count];
            for (int k = 0; k < count; k++)
            {
                objects.Add(new object());
            }
            for (int k = 0; k < 100; k++)
            {
                object o = TestData.RandomListValue<object>(objects);

                int index = objects.IndexOf(o);
                returns[index]++;
            }

            foreach (int i in returns)
            {
                Assert.Greater(i, 0);
            }
        }

        [Test]
        public void RandomEnumValue()
        {
            double runs = 100;
            int[] counts = {0, 0, 0};
            for (int k = 0; k < runs; k++)
            {
                TestEnum value = TestData.RandomEnumValue<TestEnum>();
                counts[(int) value]++;
            }

            foreach (int count in counts)
            {
                Assert.Greater(count, runs*.1);
            }
        }

        [Test]
        public void RandomDateTime_MinDateTime()
        {
            DateTime now = DateTime.Now;
            DateTime value = TestData.RandomDateTime(DateTime.MinValue, now);

            Assert.GreaterOrEqual(now, value);
            Assert.LessOrEqual(value, now);

        }

        [Test]
        public void RandomDateTime()
        {
            DateTime min = DateTime.Now;
            DateTime max = DateTime.Now + TimeSpan.FromDays(5);
            Dictionary<long, long> tickValues = new Dictionary<long, long>();
            int runs = 100;
            int maxDuplicates = runs/20;
            int duplicates = 0;

            for (int k = 0; k < runs; k++)
            {
                DateTime value = TestData.RandomDateTime(min, max);

                Assert.LessOrEqual(value, max);
                Assert.GreaterOrEqual(value, min);
                // not important that these Millisecond components are equal, but that's how the method is documented 
                Assert.AreEqual(min.Millisecond, value.Millisecond);

                if (tickValues.ContainsKey(value.Ticks))
                {
                    duplicates++;
                }
                tickValues[value.Ticks] = value.Ticks;
            }

            Assert.LessOrEqual(duplicates, maxDuplicates);
        }
    }
}