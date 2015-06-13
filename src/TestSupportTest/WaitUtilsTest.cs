using System;
using NUnit.Framework;

namespace Coditate.TestSupport
{
    [TestFixture]
    public class WaitUtilsTest
    {
        [Test]
        public void WaitTillTrue()
        {
            DateTime start = DateTime.Now;
            var wait = new WaitUtils
                {
                    SleepTime = TimeSpan.FromMilliseconds(100),
                    Timeout = TimeSpan.FromMilliseconds(1000)
                };
            int count = 0;
            int limit = 5;
            wait.WaitTillTrue(delegate
                {
                    return ++count > limit;
                });

            Assert.GreaterOrEqual(DateTime.Now, start + TimeSpan.FromMilliseconds(500));
        }

        [Test]
        public void WaitTillTrue_Timeout()
        {
            DateTime start = DateTime.Now;
            var wait = new WaitUtils
            {
                SleepTime = TimeSpan.FromMilliseconds(100),
                Timeout = TimeSpan.FromMilliseconds(500)
            };
            try
            {
                wait.WaitTillTrue(delegate
                {
                    return false;
                });

                Assert.Fail();
            } catch (TimeoutException)
            {
                Assert.GreaterOrEqual(DateTime.Now, start + TimeSpan.FromMilliseconds(500));
            }
        }
    }
}