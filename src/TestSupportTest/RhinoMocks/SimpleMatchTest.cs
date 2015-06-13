using System;
using System.Collections.Generic;
using Coditate.Common.Util;
using NUnit.Framework;

namespace Coditate.TestSupport.RhinoMocks
{
    [TestFixture]
    public class SimpleMatchTest
    {
        private class A
        {
            public string Name { get; set; }
        }

        private class B
        {
        }

        private SimpleMatch matcher;

        [Test]
        public void Eval()
        {
            A a1 = new A
                {
                    Name = RandomData.AlphaNumericString(10, true)
                };
            A a2 = new A
                {
                    Name = a1.Name
                };
            matcher = new SimpleMatch(a1);

            Assert.IsTrue(matcher.Eval(a2));

            a2.Name = RandomData.AlphaNumericString(10, true);
            Assert.IsFalse(matcher.Eval(a2));
        }

        [Test]
        public void Eval_ListAndNonList()
        {
            matcher = new SimpleMatch(new List<string>());
            bool equal = matcher.Eval(DateTime.Now  );

            Assert.AreEqual(false, equal);
            Assert.IsTrue(matcher.Message.Contains("The first object is IEnumerable"));
        }

        [Test]
        public void Eval_MismatchedTypes()
        {
            A a = new A();
            B b = new B();
            matcher = new SimpleMatch(a);

            Assert.IsFalse(matcher.Eval(b));
        }
    }
}