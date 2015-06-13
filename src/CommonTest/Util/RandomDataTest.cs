using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Coditate.Common.IO;
using NUnit.Framework;
using System.Linq;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class RandomDataTest
    {
        [Test]
        public void Binary()
        {
            int length = RandomData.Generator.Next(1000);
            byte[] b = RandomData.Binary(length);

            Assert.AreEqual(length, b.Length);
        }

        [Test]
        public void NumericString()
        {
            int length = RandomData.Generator.Next(100);
            string s = RandomData.NumericString(length);

            Assert.AreEqual(length, s.Length);

            foreach (char c in s)
            {
                Assert.IsTrue(Char.IsDigit(c));
            }
        }

        [Test]
        public void String()
        {
            int length = RandomData.Generator.Next(1000);
            string s1 = RandomData.String(length);

            Assert.AreEqual(length, s1.Length);

            // encode and decode to ensure portability of random string (removal of surrogate chars and other garbage)
            byte[] b = Encoding.UTF8.GetBytes(s1);
            string s2 = Encoding.UTF8.GetString(b);

            Assert.AreEqual(length, s2.Length);
            Assert.AreEqual(s1, s2);
        }

        [Test]
        public void StringList()
        {
            int count = 3;
            int minLength = RandomData.Generator.Next(1, 100);
            int maxLength = RandomData.Generator.Next(minLength, minLength + 100);
            bool includeSpaces = RandomData.Bool();
            bool lowercase = RandomData.Bool();

            var list = RandomData.StringList(count, minLength, maxLength, includeSpaces, lowercase);

            Assert.IsFalse(lowercase & list.Where(s => s != s.ToLowerInvariant()).Any());
            Assert.IsFalse(!includeSpaces & list.Where(s => s.Contains(' ')).Any());
            Assert.IsFalse(list.Where(s => s.Length > maxLength).Any());
            Assert.IsFalse(list.Where(s => s.Length < minLength).Any());
            Assert.IsFalse(list.Distinct().Count() != list.Count);
        }

        [Test]
        public void Duration()
        {
            int minTicks = RandomData.Int();
            int maxTicks = RandomData.Generator.Next(minTicks, int.MaxValue);

            var duration = RandomData.Duration(minTicks, maxTicks);

            Assert.GreaterOrEqual(TimeSpan.FromTicks(maxTicks), duration);
            Assert.LessOrEqual(TimeSpan.FromTicks(minTicks), duration);
        }

        [Test]
        public void AsciiString()
        {
            int length = RandomData.Generator.Next(0, 10000);
            string randomString = RandomData.AsciiString(length);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(c >= 0x20 || c <= 0x7E, c.ToString());
            }
        }

        [Test]
        public void AlphaNumericString()
        {
            int length = RandomData.Generator.Next(0, 10000);
            string randomString = RandomData.AlphaNumericString(length, true);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(c == ' ' || char.IsLetterOrDigit(c), c.ToString());
            }
        }

        [Test]
        public void AlphaNumericString_NoSpaces()
        {
            int length = RandomData.Generator.Next(0, 10000);
            string randomString = RandomData.AlphaNumericString(length, false);

            Assert.AreEqual(length, randomString.Length);
            foreach (char c in randomString)
            {
                Assert.IsTrue(char.IsLetterOrDigit(c), c.ToString());
            }
        }

        [Test]
        public void AlphaNumericString_NoConsecutiveSpaces()
        {
            int length = RandomData.Generator.Next(0, 10000);
            string randomString = RandomData.AlphaNumericString(length, true);

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
        public void Bool()
        {
            double runs = 100;
            int count = (int)runs / 2;
            for (int k = 0; k < runs; k++)
            {
                if (RandomData.Bool())
                {
                    count++;
                }
                else
                {
                    count--;
                }
            }

            // verify that true and false are each returned at least 10% of the time
            Assert.Greater(count, runs * .1);
            Assert.Less(count, runs * .9);
        }

        [Test]
        public void ListValue()
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
                object o = RandomData.ListValue<object>(objects);

                int index = objects.IndexOf(o);
                returns[index]++;
            }

            foreach (int i in returns)
            {
                Assert.Greater(i, 0);
            }
        }

        [Test]
        public void EnumValue()
        {
            double runs = 100;
            int[] counts = { 0, 0, 0 };
            for (int k = 0; k < runs; k++)
            {
                TestEnum value = RandomData.EnumValue<TestEnum>();
                counts[(int)value]++;
            }

            foreach (int count in counts)
            {
                Assert.Greater(count, runs * .1);
            }
        }

        [Test]
        public void DateTime_MinDateTime()
        {
            DateTime now = DateTime.Now;
            DateTime value = RandomData.DateTime(DateTime.MinValue, now);

            Assert.GreaterOrEqual(now, value);
            Assert.LessOrEqual(value, now);

        }

        [Test]
        public void TempFile()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.Delete(path);
                int size = RandomData.Generator.Next(100, 10000);

                FileInfo file = RandomData.TempFile(path, size);

                Assert.IsTrue(file.Exists);
                Assert.AreEqual(size, file.Length);
            } finally
            {
                IOUtils.Delete(new FileInfo(path));
            }
        }

        [Test]
        public void DateTimeT()
        {
            DateTime min = DateTime.Now;
            DateTime max = DateTime.Now + TimeSpan.FromDays(5);
            Dictionary<long, long> tickValues = new Dictionary<long, long>();
            int runs = 100;
            int maxDuplicates = runs / 20;
            int duplicates = 0;

            for (int k = 0; k < runs; k++)
            {
                DateTime value = RandomData.DateTime(min, max);

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

        internal enum TestEnum
        {
            First,
            Second,
            Third
        }

    }
}