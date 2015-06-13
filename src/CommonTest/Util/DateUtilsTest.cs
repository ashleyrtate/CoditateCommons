using System;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class DateUtilsTest
    {
        [Test]
        public void RoundToYear()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Year);

            Assert.AreEqual(first.Year, second.Year);
            Assert.AreEqual(1, second.Month);
            Assert.AreEqual(1, second.Day);
            Assert.AreEqual(0, second.Hour);
            Assert.AreEqual(0, second.Minute);
            Assert.AreEqual(0, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }

        [Test]
        public void RoundToMonth()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Month);

            Assert.AreEqual(first.Month, second.Month);
            Assert.AreEqual(1, second.Day);
            Assert.AreEqual(0, second.Hour);
            Assert.AreEqual(0, second.Minute);
            Assert.AreEqual(0, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }

        [Test]
        public void RoundToDay()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Day);

            Assert.AreEqual(first.Day, second.Day);
            Assert.AreEqual(0, second.Hour);
            Assert.AreEqual(0, second.Minute);
            Assert.AreEqual(0, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }

        [Test]
        public void RoundToHour()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Hour);

            Assert.AreEqual(first.Hour, second.Hour);
            Assert.AreEqual(0, second.Minute);
            Assert.AreEqual(0, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }
        
        [Test]
        public void RoundToMinute()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Minute);

            Assert.AreEqual(first.Minute, second.Minute);
            Assert.AreEqual(0, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }

        [Test]
        public void RoundToSecond()
        {
            DateTime first = DateTime.Now;
            DateTime second = DateUtils.Round(first, DateRounding.Second);

            Assert.AreEqual(first.Second, second.Second);
            Assert.AreEqual(0, second.Millisecond);
        }


        [Test]
        public void RoundToSecond_TimeStamp()
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 1, 501);
            TimeSpan ts2 = DateUtils.Round(ts, DateRounding.Second);

            Assert.AreEqual(2, ts2.Seconds);
        }

        [Test]
        public void RoundToMinute_TimeStamp()
        {
            TimeSpan ts = new TimeSpan(0, 0, 1, 31);
            TimeSpan ts2 = DateUtils.Round(ts, DateRounding.Minute);

            Assert.AreEqual(2, ts2.Minutes);
        }
    }
}