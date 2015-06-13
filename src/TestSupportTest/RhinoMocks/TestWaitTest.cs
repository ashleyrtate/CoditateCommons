using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Coditate.TestSupport.RhinoMocks
{
    [TestFixture]
    public class TestWaitTest
    {
        private IMyInterface myObject;
        private MockRepository repo;

        public interface IMyInterface
        {
            void MyMethod(string s);
        }

        [SetUp]
        public void SetUp()
        {
            repo = new MockRepository();
            myObject = repo.StrictMock<IMyInterface>();

            repo.Record();
        }

        [Test]
        public void Wait()
        {
            myObject.MyMethod(null);
            TestWait.ForLastCall<string>();

            repo.ReplayAll();

            bool methodInvoked = false;
            ParameterizedThreadStart callback = delegate
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    myObject.MyMethod(null);
                    methodInvoked = true;
                };

            TestThreadRunner runner = new TestThreadRunner();
            runner.AddThread(callback, null);
            runner.Run();

            TestWait.Wait(TimeSpan.FromSeconds(5));

            Assert.IsTrue(methodInvoked);
        }

        /// <summary>
        /// Verifies that wait method times out after specified duration
        /// if expected method is not invoked.
        /// </summary>
        [Test]
        public void Wait_TimeExpires()
        {
            myObject.MyMethod(null);
            TestWait.ForLastCall<string>();

            repo.ReplayAll();
            
            DateTime start = DateTime.Now;
            DateTime end;
            try
            {
                TestWait.Wait(TimeSpan.FromSeconds(1));
                Assert.Fail("Expected TimeoutException");
            }
            catch (TimeoutException)
            {
                // ignore
            }

            end = DateTime.Now;
            TimeSpan waitDuration = end - start;
            Assert.Greater(waitDuration, TimeSpan.FromMilliseconds(900));
        }
    }
}