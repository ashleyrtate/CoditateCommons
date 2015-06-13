using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class EmailUtilsTest
    {
        [Test]
        public void GetAddresses()
        {
            string[] emails = {"a@example.com", "b@example.com", "c@example.com"};
            string invalid;

            foreach (string separator in EmailUtils.DefaultSeparators)
            {
                List<MailAddress> addresses = EmailUtils.GetAddresses(StringUtils.Join(separator, emails),
                                                                      EmailUtils.DefaultSeparators, out invalid);
                Assert.AreEqual(emails.Length, addresses.Count);

                PropertyMatcher.MatchResult result = PropertyMatcher.AreEqual(emails, addresses.Select(a => a.Address));
                Assert.IsTrue(result.Equal, result.Message);

                Assert.IsNull(invalid);
            }
        }

        [Test]
        public void GetAddresses_Invalid()
        {
            string[] emails = {"a@example.com", "xyz"};
            string invalid;


            List<MailAddress> addresses = EmailUtils.GetAddresses(StringUtils.Join(",", emails),
                                                                  EmailUtils.DefaultSeparators, out invalid);

            Assert.AreEqual(1, addresses.Count);
            Assert.AreEqual("xyz", invalid);
        }

        [Test]
        public void GetAddresses_DiscardDuplicate()
        {
            string[] emails = { "a@example.com", "a@example.com" };
            string invalid;


            List<MailAddress> addresses = EmailUtils.GetAddresses(StringUtils.Join(",", emails),
                                                                  EmailUtils.DefaultSeparators, out invalid);

            Assert.AreEqual(1, addresses.Count);
            Assert.IsNull(invalid);
        }

        [Test]
        public void GetAddresses_IgnoreEmpty()
        {
            string[] emails = { "a@example.com", " " };
            string invalid;

            List<MailAddress> addresses = EmailUtils.GetAddresses(StringUtils.Join(",", emails),
                                                                  EmailUtils.DefaultSeparators, out invalid);

            Assert.AreEqual(1, addresses.Count);
            Assert.IsNull(invalid);
        }
    }
}