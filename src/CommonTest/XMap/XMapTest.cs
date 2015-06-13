using System.IO;
using System.Xml;
using Coditate.Common.IO;
using Coditate.Common.Util;
using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.XMap
{
    [TestFixture]
    public class XMapTest
    {
        private static readonly FileInfo ReplaceBeforeFile = new FileInfo(@".\XMap\XMapTest-ReplaceIn.xml");
        private static readonly FileInfo ReplaceAfterFile = new FileInfo(@".\XMap\XMapTest-ReplaceOut.xml");
        private static readonly FileInfo MapFile = new FileInfo(@".\XMap\XMapTest-Replace.xmap");
        private XMap mapper;
        private string tempPath;
        private string replaceFilePath;

        [SetUp]
        public void SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), typeof (XMapTest).Name);
            IOUtils.Delete(new DirectoryInfo(tempPath), true);

            Directory.CreateDirectory(tempPath);
            replaceFilePath = Path.Combine(tempPath, ReplaceBeforeFile.Name);

            mapper = new XMap();
            mapper.AddProperty("MyProperty",
                               "my property value");

            for (int k = 0; k < 3; k++)
            {
                mapper.AddProperty(RandomData.AlphaNumericString(15, false),
                                   RandomData.AlphaNumericString(100, true));
            }
        }

        [TearDown]
        public void TearDown()
        {
            IOUtils.Delete(new DirectoryInfo(tempPath), true);
        }

        [Test]
        public void ExpandValue()
        {
            string name = mapper.GetPropertyNames()[0];
            string expectedValue = mapper.GetProperty(name);
            string expandableValue = "$(" + name + ")";

            string actualValue = mapper.ExpandValue(expandableValue);

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void ExpandValue_MultipleProperties()
        {
            string expectedValue = "";
            string expandableValue = "";
            foreach (string propertyName in mapper.GetPropertyNames())
            {
                expectedValue += mapper.GetProperty(propertyName);
                expandableValue += "$(" + propertyName + ")";
            }
            string actualValue = mapper.ExpandValue(expandableValue);

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void ExpandValue_Recursive()
        {
            string firstName = "MyProperty1";
            string secondName = "MyProperty2";
            string firstValue = "filler-$(MyProperty2)-more filler";
            string secondValue = "property value 2";
            mapper.AddProperty(firstName, firstValue);
            mapper.AddProperty(secondName, secondValue);

            string expectedValue = firstValue.Replace("$(MyProperty2)", secondValue);
            string expandableValue = "$(" + firstName + ")";

            string actualValue = mapper.ExpandValue(expandableValue);

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void GetPathRoot()
        {
            string[] paths = {"", "/", "//", "a/", "//a", "/a/", "//a/", "/a/a", "//a/a/"};
            string[] roots = {"", "/", "//", "a", "//a", "/a", "//a", "/a", "//a"};

            for (int k = 0; k < paths.Length; k++)
            {
                string root = mapper.GetPathRoot(paths[k]);
                Assert.AreEqual(roots[k], root);
            }
        }

        [Test]
        public void UpdateXmlDocument()
        {
            string xml = @"<a><b att=""aaa"" /></a>";
            string expectedXml = @"<a><b att=""zzz"" /></a>";
            string attPath = @"//a/b/@att";
            string value = "zzz";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            mapper.Replace(xmlDoc, attPath, value);

            Assert.AreEqual(expectedXml, xmlDoc.OuterXml);
        }

        [Test]
        public void UpdateXmlFile_OutputNewPath()
        {
            mapper.UpdateNodes(ReplaceBeforeFile.FullName, replaceFilePath, MapFile.FullName);

            using (TextReader first = ReplaceAfterFile.OpenText(), second = new StreamReader(replaceFilePath))
            {
                AssertEx.AreEqual(first, second);
            }
        }

        [Test]
        public void UpdateXmlFile_OutputSamePath()
        {
            ReplaceBeforeFile.CopyTo(replaceFilePath);

            mapper.UpdateNodes(replaceFilePath, MapFile.FullName);

            // todo: should implement support for comparing two xml documents element by element
            using (TextReader first = ReplaceAfterFile.OpenText(), second = new StreamReader(replaceFilePath))
            {
                AssertEx.AreEqual(first, second);
            }
        }
    }
}