using System;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class PropertyUtilsTest
    {
        [Test]
        public void CopyProperties()
        {
            var a1 = new A
                {
                    DateVal = RandomData.DateTime(DateTime.Now, DateTime.Now.AddDays(100)),
                    IntVal = RandomData.Int(),
                    StringVal = RandomData.AsciiString(100)
                };
            var a2 = new A();

            PropertyUtils.CopyProperties(a1, a2);

            Assert.AreEqual(a1.DateVal, a2.DateVal);
            Assert.AreEqual(a1.IntVal, a2.IntVal);
            Assert.AreEqual(a1.StringVal, a2.StringVal);
            Assert.AreEqual(a1.Child, a2.Child);
        }

        [Test]
        public void CopyProperties_Nullable()
        {
            var a1 = new A();
            var b1 = new B();

            PropertyUtils.CopyProperties(a1, b1, "StringVal2");
            Assert.AreEqual(0, b1.IntVal2);

            PropertyUtils.CopyProperties(b1, a1, "StringVal2");
            Assert.AreEqual(0, a1.IntVal2);

            a1.IntVal2 = 99;

            PropertyUtils.CopyProperties(a1, b1, "StringVal2");
            Assert.AreEqual(a1.IntVal2, b1.IntVal2);

            b1.IntVal2 = 88;

            PropertyUtils.CopyProperties(b1, a1, "StringVal2");
            Assert.AreEqual(a1.IntVal2, b1.IntVal2);
        }

        [Test]
        public void CopyProperties_DifferentType()
        {
            var a = new A
                {
                    DateVal = RandomData.DateTime(DateTime.Now, DateTime.Now.AddDays(100)),
                    IntVal = RandomData.Int(),
                    StringVal = RandomData.AsciiString(100),
                    Child = new A()
                };
            var b = new B();

            PropertyUtils.CopyProperties(a, b, "StringVal2");

            Assert.AreEqual(a.DateVal, b.DateVal);
            Assert.AreEqual(a.IntVal, b.IntVal);
            Assert.AreEqual(a.StringVal, b.StringVal);
            Assert.AreEqual(a.Child, b.Child);
        }

        [Test,
         ExpectedException(typeof (InvalidOperationException),
             ExpectedMessage =
                 "Expected property 'StringVal2' not found on type 'Coditate.Common.Util.PropertyUtilsTest+B'")]
        public void CopyProperties_MissingProperty()
        {
            var a = new A();
            var b = new B();

            PropertyUtils.CopyProperties(a, b);
        }

        [Test]
        public void CopyProperties_MissingProperty_IgnoreMissing()
        {
            var a = new A();
            var b = new B();

            PropertyUtils.CopyProperties(a, b, PropertyMatch.IgnoreMissing);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void FindMissingProperty()
        {
            var a = new A();
            PropertyUtils.FindValue(a, "MissingProperty");
        }

        [Test]
        public void FindNestedProperty()
        {
            var a = new A();
            a.Child = new A();
            a.Child.Child = new A();
            a.Child.Child.StringVal = "My String";
            Object propVal = PropertyUtils.FindValue(a, "Child.Child.StringVal");
            bool equal = a.Child.Child.StringVal.Equals(propVal);
            Assert.IsTrue(equal);
        }

        [Test]
        public void FindPropertyWithEmptyPath()
        {
            var a = new A();
            Object propVal = PropertyUtils.FindValue(a, "");
            Assert.IsTrue(a == propVal);
        }

        [Test]
        public void ReturnFalseForInvalidNestedPath()
        {
            Assert.IsFalse(PropertyUtils.IsPathValid(typeof (A),
                                                     "Child.Child.BoolVal"));
        }

        [Test]
        public void ReturnFalseForInvalidPath()
        {
            Assert.IsFalse(PropertyUtils.IsPathValid(typeof (A), "MissingProperty"));
        }

        [Test]
        public void ReturnNullForNullNestedProperty()
        {
            var a = new A();
            a.Child = new A();
            a.Child.Child = new A();
            Object propVal = PropertyUtils.FindValue(a, "Child.Child.StringVal");
            Assert.IsNull(propVal);
        }

        [Test]
        public void ReturnNullForNullObject()
        {
            Object propVal = PropertyUtils.FindValue(null, "stringval");
            Assert.IsNull(propVal);
        }

        [Test]
        public void ReturnNullForNullPropertyOnPath()
        {
            var a = new A();
            Object propVal = PropertyUtils.FindValue(a, "Child.Child.StringVal");
            Assert.IsNull(propVal);
        }

        [Test]
        public void ReturnTrueForEmptyPath()
        {
            Assert.IsTrue(PropertyUtils.IsPathValid(typeof (A), ""));
        }

        [Test]
        public void ReturnTrueForValidNestedPath()
        {
            Assert.IsTrue(PropertyUtils.IsPathValid(typeof (A),
                                                    "Child.Child.StringVal"));
        }

        [Test]
        public void ReturnTrueForValidPath()
        {
            Assert.IsTrue(PropertyUtils.IsPathValid(typeof (A), "StringVal"));
        }

        /// <summary>
        /// A generic bean to use in unit tests.
        /// </summary>
        private class A
        {
            public string StringVal { get; set; }

            public string StringVal2 { get; set; }

            public int IntVal { get; set; }

            public int? IntVal2 { get; set; }

            public DateTime DateVal { get; set; }

            public A Child { get; set; }
        }

        private class B
        {
            public string StringVal { get; set; }

            public int IntVal { get; set; }

            public int IntVal2 { get; set; }

            public DateTime DateVal { get; set; }

            public A Child { get; set; }
        }
    }
}