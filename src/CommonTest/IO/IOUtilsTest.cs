using System;
using System.IO;
using Coditate.Common.Util;
using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.IO
{
    [TestFixture]
    public class IOUtilsTest
    {
        private string tempDirPath;

        [SetUp]
        public void SetUp()
        {
            tempDirPath = Path.Combine(Path.GetTempPath(), "IOUtilsTest");
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
            Directory.CreateDirectory(tempDirPath);
        }

        [TearDown]
        public void TearDown()
        {
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
        }

        [Test]
        public void GetDirectoryName()
        {
            string directoryName = "abc123";
            string directoryPath = Path.Combine(tempDirPath, directoryName);

            Directory.CreateDirectory(directoryPath);

            DirectoryInfo newDirectory1 = IOUtils.GetUniqueDirectory(directoryPath);
            newDirectory1.Create();
            DirectoryInfo newDirectory2 = IOUtils.GetUniqueDirectory(directoryPath);
            newDirectory2.Create();
            DirectoryInfo newDirectory3 = IOUtils.GetUniqueDirectory(directoryPath);
            newDirectory3.Create();

            Assert.AreEqual(newDirectory1.Name, directoryName + "_00");
            Assert.AreEqual(newDirectory2.Name, directoryName + "_01");
            Assert.AreEqual(newDirectory3.Name, directoryName + "_02");
        }

        [Test]
        public void TransferData()
        {
            int byteCount = RandomData.Generator.Next(500000);
            var fromStream = new MemoryStream(RandomData.Binary(byteCount));
            var toStream = new MemoryStream(byteCount);

            Console.WriteLine("Transferring " + byteCount + " bytes");

            IOUtils.TransferData(fromStream, toStream);

            var result = PropertyMatcher.AreEqual(fromStream.ToArray(), toStream.ToArray());
            Assert.IsTrue(result.Equal, result.Message);
        }

        [Test]
        public void IsNetworkPath()
        {
            Assert.IsTrue(IOUtils.IsNetworkPath(@"\\localhost\"));

            Assert.IsFalse(IOUtils.IsNetworkPath(@"c:\"));

            Assert.IsFalse(IOUtils.IsNetworkPath(@"\"));
        }
    }
}