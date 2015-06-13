using System;
using Coditate.Common.IO;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class FormatUtilsTest
    {
        [Test]
        public void BytesAsMegaBytes()
        {
            string expectedNoLabel = "1.0";
            string formatted = FormatUtils.BytesAsMegaBytes(IOUtils.BytesPerMegabyte, false);

            Assert.AreEqual(expectedNoLabel, formatted);

            string expectedWithLabel = "1.0 MB";
            formatted = FormatUtils.BytesAsMegaBytes(IOUtils.BytesPerMegabyte, true);

            Assert.AreEqual(expectedWithLabel, formatted);

            ulong a = 1;
            FormatUtils.BytesAsMegaBytes((long)a, true);
        }

        /// <summary>
        /// Test date formatting with a non-DST DateTime.
        /// </summary>
        [Test]
        public void DateAsLocal_StandardTime()
        {
            var date = new DateTime(2000, 1, 1);
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

            // utc date is adjusted to local
            string formatted = FormatUtils.DateAsLocal(date.ToUniversalTime(), offset, "yyyy-MM-dd hh:mm:ss");
            Assert.AreEqual("2000-01-01 12:00:00", formatted);
        }

        [Test]
        public void DateAsLocal_NonUTCTime()
        {
            var date = new DateTime(2000, 1, 1);
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

            // local date is formatted without adjustment
            string formatted = FormatUtils.DateAsLocal(date, offset, "yyyy-MM-dd hh:mm:ss");
            Assert.AreEqual("2000-01-01 12:00:00", formatted);
        }

        /// <summary>
        /// Test date formatting with a DST DateTime
        /// </summary>
        [Test]
        public void DateAsLocal_DstTime()
        {
            var date = new DateTime(2000, 6, 1);
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

            // utc date is adjusted to local
            string formatted = FormatUtils.DateAsLocal(date.ToUniversalTime(), offset, "yyyy-MM-dd hh:mm:ss");
            Assert.AreEqual("2000-06-01 12:00:00", formatted);
        }

        /// <summary>
        /// Test date formatting with a DST DateTime
        /// </summary>
        [Test]
        public void DateAsLocalAutoFormat()
        {
            var date = new DateTime(2000, 6, 1);
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

            // utc date is adjusted to local
            string formatted = FormatUtils.DateAsLocal(date.ToUniversalTime(), offset, "MMM d \"'\"yy \"at\" h:mm tt");
            Assert.AreEqual("Jun 1 '00 at 12:00 AM", formatted);
        }

        [Test]
        public void DateToLocalRelative()
        {
            DateTime date = DateTime.UtcNow;
            string formatted = FormatUtils.DateToLocalRelative(date, TimeSpan.Zero);
            Assert.AreEqual("0 min ago", formatted);
        }

        [Test]
        public void DateToLocalRelative_Internal()
        {
            var then = new DateTime(2000, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = then + TimeSpan.FromDays(366);

            var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            string formatted = FormatUtils.DateToLocalRelative(then, now, utcOffset);
            Assert.AreEqual("Jan 14 '00 at 7:00 PM", formatted);
        }

        [Test]
        public void ZipCodeForDisplay()
        {
            var formatted = FormatUtils.ZipCodeForDisplay(null);
            Assert.IsNull(formatted);

            formatted = FormatUtils.ZipCodeForDisplay("12345");
            Assert.AreEqual("12345", formatted);

            formatted = FormatUtils.ZipCodeForDisplay("123456789");
            Assert.AreEqual("12345-6789", formatted);

            formatted = FormatUtils.ZipCodeForDisplay("12345-6789");
            Assert.AreEqual("12345-6789", formatted);
        }
    }
}