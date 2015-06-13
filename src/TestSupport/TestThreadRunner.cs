using System.Collections.Generic;
using System.Threading;

namespace Coditate.TestSupport
{
    /// <summary>
    /// Supports implementation of multi-threaded unit tests.
    /// </summary>
    /// <remarks>
    /// To use this class invoke either
    /// <see cref="AddThreads"/> or <see cref="AddThread"/> to register callback
    /// methods that implement the desired test functionality. Then invoke <see cref="Run"/>
    /// or <see cref="RunAsync"/> to kick off the test threads. 
    /// 
    /// <para>Example: Here is a very simple test that uses multiple threads to 
    /// add strings to a <see cref="List{T}"/> and verifies that the expected number of 
    /// strings was added.
    /// </para>
    /// <code>
    ///   [Test]
    ///   public void SampleUsageTest()
    ///   {
    ///      int expectedStringCount = 10;
    ///      int threadCount = 3;
    ///      List{string} myStrings = new List{string}();
    ///
    ///      // define an anonymous delegate
    ///      ParameterizedThreadStart callback = delegate
    ///      {
    ///         // add strings to collection
    ///         while (myStrings.Count &lt; expectedStringCount)
    ///         {
    ///            lock (((ICollection) myStrings).SyncRoot)
    ///            {
    ///               myStrings.Add("abc123");
    ///            }
    ///         }
    ///      };
    ///
    ///      // setup and start thread runner
    ///      TestThreadRunner runner = new TestThreadRunner();
    ///      runner.AddThreads(callback, null, threadCount);
    ///      runner.Run();
    ///
    ///      // verify desired string count
    ///      Assert.GreaterOrEqual(myStrings.Count, expectedStringCount);
    ///   }
    /// </code>
    /// </remarks>
    public class TestThreadRunner
    {
        /// <summary>
        /// Holds a reference to 
        /// </summary>
        private class ThreadStartWrapper
        {
            public Semaphore semaphore;
            public object state;
            public ParameterizedThreadStart threadStart;

            public ThreadStartWrapper(ParameterizedThreadStart threadStart, object state)
            {
                this.threadStart = threadStart;
                this.state = state;
            }
        }

        private int startedWorkThreads;
        private IList<ThreadStartWrapper> startWrappers = new List<ThreadStartWrapper>();
        private ThreadPriority workerPriority = ThreadPriority.Normal;
        private List<Thread> workThreads = new List<Thread>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestThreadRunner"/> class.
        /// </summary>
        public TestThreadRunner()
        {
            WorkerPriority = ThreadPriority.Normal;
        }

        /// <summary>
        /// Gets or sets the worker priority.
        /// </summary>
        /// <value>The worker priority.</value>
        public ThreadPriority WorkerPriority { get; set; }

        private int WorkThreadCount
        {
            get { return startWrappers.Count; }
        }

        /// <summary>
        /// Adds a thread to the pool that will be started.
        /// </summary>
        /// <param name="threadStart">The callback method to run.</param>
        /// <param name="state">The state object to pass to the callback method.</param>
        public void AddThread(ParameterizedThreadStart threadStart, object state)
        {
            // todo: see if the below works
            //var wrapper = new {ThreadStart = threadStart, State = state, Semaphore = (Semaphore)null};
            ThreadStartWrapper startWrapper = new ThreadStartWrapper(threadStart, state);
            startWrappers.Add(startWrapper);
        }

        /// <summary>
        /// Add multiple callbacks that will each be invoked on their
        /// own threads.
        /// </summary>
        /// <param name="threadStart">The callback method to run.</param>
        /// <param name="state">The state object to pass to the callback method.</param>
        /// <param name="count">The number of threads which will invoke the callback.</param>
        /// <remarks>
        /// The effect of this method is the same as calling <see cref="AddThread(ParameterizedThreadStart, object)"/>
        /// count times and passing the same state object with each call.
        /// </remarks>
        public void AddThreads(ParameterizedThreadStart threadStart, object state, int count)
        {
            for (int k = 0; k < count; k++)
            {
                AddThread(threadStart, state);
            }
        }

        /// <summary>
        /// Calls each registered callback delegate in its own thread.
        /// </summary>
        /// <remarks>
        /// This method does not return until all registered callback methods return. 
        /// </remarks>
        public void Run()
        {
            RunAsync();

            foreach (Thread thread in workThreads)
            {
                thread.Join();
            }
        }

        /// <summary>
        /// Starts each registered callback delegate in its own thread and returns immediately.
        /// </summary>
        public void RunAsync()
        {
            // semaphore lets us queue up all the worker threads and release them at once
            Semaphore semaphore = new Semaphore(0, WorkThreadCount);

            StartWorkThreads(semaphore);

            Thread.Sleep(100);
            semaphore.Release(WorkThreadCount);
            Thread.Sleep(100);
        }

        private void StartWorkThreads(Semaphore semaphore)
        {
            for (int k = 0; k < WorkThreadCount; k++)
            {
                startWrappers[k].semaphore = semaphore;
                Thread thread = new Thread(DoWork);
                thread.Priority = workerPriority;
                thread.IsBackground = true;
                thread.Start(startWrappers[k]);

                workThreads.Add(thread);
            }

            while (startedWorkThreads < WorkThreadCount)
            {
                Thread.Sleep(0);
            }
        }

        private void DoWork(object state)
        {
            Interlocked.Increment(ref startedWorkThreads);

            ThreadStartWrapper startWrapper = (ThreadStartWrapper) state;
            startWrapper.semaphore.WaitOne();

            startWrapper.threadStart.Invoke(startWrapper.state);
        }
    }
}