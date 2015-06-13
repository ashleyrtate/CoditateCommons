using System;
using System.Threading;
using Rhino.Mocks;

namespace Coditate.TestSupport.RhinoMocks
{
    /// <summary>
    /// Allows test to wait for mock method invocation expected to occur on a different thread.
    /// </summary>
    /// <remarks>
    /// To use this class invoke
    /// <see cref="ForLastCall"/> with the generic types of the last mocked method. Next, invoke the
    /// mocked method on a new thread. Finally, invoke <see cref="Wait"/>.
    /// 
    /// <para>Example:
    /// </para>
    /// <code>
    /// [Test]
    ///   public void SampleUsageTest()
    ///   {
    ///     string someText = "abc";
    ///     IMyInterface myObject = MockRepository.GenerateMock&lt;IMyInterface&gt;();
    /// 
    ///     ParameterizedThreadStart callback = delegate
    ///      {
    ///         myObject.MyMethod(someText);
    ///      };
    ///
    ///     // set mock expectations
    ///     myObject.MyMethod(someText);
    ///     TestWait.ForLastCall&lt;string&gt;();
    /// 
    ///     // setup and start thread runner
    ///     TestThreadRunner runner = new TestThreadRunner();
    ///     runner.AddThread(callback, null);
    ///     runner.Run();
    /// 
    ///     // call won't return until myObject.MyMethod(someText) is invoked
    ///     TestWait.Wait(TimeSpan.FromSeconds(1));
    ///   }
    /// </code>
    /// </remarks>
    public class TestWait
    {
        private static bool testCompleted;
        private static int threadId;

        /// <summary>
        /// Registers for notification of the last mock method invocation using LastCall.Do().
        /// </summary>
        /// <typeparam name="T1">The first type of the last mock method.</typeparam>
        /// <typeparam name="T2">The second type of the last mock method</typeparam>
        public static void ForLastCall<T1, T2>()
        {
            Action<T1, T2> completionHandler = delegate { testCompleted = true; };
            // registers callback that allows us to deterministically know when test is finished
            LastCall.Do(completionHandler);
            threadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Registers for notification of the last mock method invocation using LastCall.Do().
        /// </summary>
        /// <typeparam name="T1">The first type of the last mock method.</typeparam>
        public static void ForLastCall<T1>()
        {
            Action<T1> completionHandler = delegate { testCompleted = true; };
            ForLastCall(completionHandler);
        }

        private static void ForLastCall(Delegate action)
        {
            // registers callback that allows us to deterministically know when test is finished
            LastCall.Do(action);
            threadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Waits for the registered call back to occur.
        /// </summary>
        /// <param name="maxWait">The max time to wait.</param>
        public static void Wait(TimeSpan maxWait)
        {
            if (Thread.CurrentThread.ManagedThreadId != threadId)
            {
                string message =
                    string.Format(
                        "ForLastCall() invoked by thread {0} but Wait() invoked by thread {1}");
                throw new InvalidOperationException(message);
            }

            DateTime start = DateTime.Now;

            try
            {
                while (!testCompleted)
                {
                    Thread.Sleep(1);

                    if (DateTime.Now > start + maxWait)
                    {
                        throw new TimeoutException(
                            "Wait time expired before expected method was invoked");
                    }
                }
            }
            finally
            {
                // reset for next use
                testCompleted = false;
            }
        }
    }
}