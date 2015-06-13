using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Coditate.TestSupport
{
    [TestFixture]
    public class TestThreadRunnerTest
    {
        [Test]
        public void SampleUsageTest()
        {
            int expectedStringCount = 10;
            int threadCount = 3;
            List<string> myStrings = new List<string>();

            // define an anonymous delegate
            ParameterizedThreadStart callback = delegate
                {
                    // add strings to collection
                    while (myStrings.Count < expectedStringCount)
                    {
                        lock (((ICollection) myStrings).SyncRoot)
                        {
                            myStrings.Add("abc123");
                        }
                    }
                };

            // setup and start thread runner
            TestThreadRunner runner = new TestThreadRunner();
            runner.AddThreads(callback, null, threadCount);
            runner.Run();

            // verify desired string count
            Assert.GreaterOrEqual(myStrings.Count, expectedStringCount);
        }

        [Test]
        public void RunTest()
        {
            int threadCount = 4;
            int requestedSwitches = 8;

            int testRuns = 5;
            int totalLoopsPerSwitch = 0;
            for (int k = 0; k < testRuns; k++)
            {
                TestThreadRunner test = new TestThreadRunner();
                test.WorkerPriority = ThreadPriority.Normal;

                int actualSwitches, loopsPerSwitch;

                runTest(test, threadCount, requestedSwitches, out actualSwitches, out loopsPerSwitch);
                totalLoopsPerSwitch += loopsPerSwitch;
                test = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(0);

                Assert.GreaterOrEqual(actualSwitches, requestedSwitches);
            }
            int averageLoopsPerSwitch = totalLoopsPerSwitch/testRuns;
            //Console.WriteLine("===========================");
            //Console.WriteLine("testRuns: " + testRuns);
            //Console.WriteLine("totalLoopsPerSwitch: " + totalLoopsPerSwitch);
            //Console.WriteLine("averageLoopsPerSwitch: " + averageLoopsPerSwitch);
        }

        private void runTest(TestThreadRunner test, int threadCount, int requestedSwitches,
                             out int actualSwitches, out int loopsPerSwitch)
        {
            int globalLoopCount = 0;
            int threadSwitchCount = 0;

            ParameterizedThreadStart callback = delegate
                {
                    int localLoopCount = 0;
                    while (threadSwitchCount < requestedSwitches)
                    {
                        if (globalLoopCount != localLoopCount)
                        {
                            Interlocked.Increment(ref threadSwitchCount);
                            localLoopCount = globalLoopCount;
                        }
                        Interlocked.Increment(ref globalLoopCount);
                        localLoopCount++;
                    }
                };

            test.AddThreads(callback, null, threadCount);
            test.Run();

            actualSwitches = threadSwitchCount;
            loopsPerSwitch = globalLoopCount/actualSwitches;

            //Console.WriteLine("threadSwitchCount: " + actualSwitches);
            //Console.WriteLine("globalLoopCount: " + globalLoopCount);
            //Console.WriteLine("loopsPerSwitch: " + loopsPerSwitch);
        }
    }
}