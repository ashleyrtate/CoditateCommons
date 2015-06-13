using System;
using System.Threading;

namespace Coditate.TestSupport
{
    /// <summary>
    /// Helper methods for waiting on things to happen in tests.
    /// </summary>
    public class WaitUtils
    {
        /// <summary>
        /// Default sleep time. Value is 1s.
        /// </summary>
        public static readonly TimeSpan DefaultSleepTime = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Default timeout time. Value is 30s.
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        private static WaitUtils defaultInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitUtils"/> class.
        /// </summary>
        public WaitUtils()
        {
            Timeout = DefaultTimeout;
            SleepTime = DefaultSleepTime;
        }

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        /// <value>The default.</value>
        public static WaitUtils Default
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new WaitUtils();
                }
                return defaultInstance;
            }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        /// <remarks>After waiting this long, timeout </remarks>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the sleep time.
        /// </summary>
        /// <value>The sleep time.</value>
        /// <remarks>Wait this long between checks of the wait-for condition</remarks>
        public TimeSpan SleepTime { get; set; }

        /// <summary>
        /// Repeatedly invokes the provided function until it returns true or the timeout value is exceeded.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        public void WaitTillTrue(Func<bool> func)
        {
            DateTime start = DateTime.Now;
            while (!func.Invoke())
            {
                Sleep(start);
            }
        }

        private void Sleep(DateTime start)
        {
            if (DateTime.Now > start + Timeout)
            {
                throw new TimeoutException("Sleep timeout expired after " + Timeout.TotalSeconds + " seconds.");
            }
            Thread.Sleep(SleepTime);
        }
    }
}