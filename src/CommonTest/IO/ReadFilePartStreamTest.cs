using System;
using System.IO;
using Coditate.Common.Util;
using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.IO
{
    [TestFixture]
    public class ReadFilePartStreamTest
    {
        private static readonly Random Rand = new Random();
        private string tempDirPath;

        [SetUp]
        public void SetUp()
        {
            tempDirPath = Path.Combine(Path.GetTempPath(), "ReadFilePartStreamTest");
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
            Directory.CreateDirectory(tempDirPath);
        }

        [TearDown]
        public void TearDown()
        {
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
        }

        private void compareReads(FileInfo file, int offset, int length)
        {
            byte[] buffer = ReadFilePart(file, offset, length);
            using (var stream = new ReadFilePartStream(file.FullName))
            {
                stream.Offset = offset;
                stream.PartLength = length;

                AssertEqual(buffer, stream);
            }
        }

        private byte[] ReadFilePart(FileInfo file, int offset, int length)
        {
            var buffer = new byte[length];
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read)
                )
            {
                stream.Position = offset;
                stream.Read(buffer, 0, length);
            }
            return buffer;
        }

        private void AssertEqual(byte[] first, Stream second)
        {
            for (int k = 0; k < first.Length; k++)
            {
                Assert.AreEqual(first[k], (byte) second.ReadByte(), "index = " + k);
            }

            Assert.AreEqual(-1, second.ReadByte());
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void OffsetChecked()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 100);

            using (var stream = new ReadFilePartStream(file.FullName))
            {
                stream.Offset = 101;
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void OffsetPlusPartLengthChecked()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 100);

            using (var stream = new ReadFilePartStream(file.FullName))
            {
                stream.Offset = 10;
                stream.PartLength = 91;
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void PartLengthChecked()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 100);

            using (var stream = new ReadFilePartStream(file.FullName))
            {
                stream.PartLength = 101;
            }
        }

        [Test]
        public void ReadEmptyFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 0);

            compareReads(file, 0, 0);
        }

        [Test]
        public void ReadFileEnd()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 100);

            compareReads(file, 50, 50);
        }

        [Test]
        public void ReadFileParts()
        {
            int size = 1000;
            FileInfo file = RandomData.TempFile(tempDirPath, size);

            int partSize = Rand.Next(size + 1);
            int offset = 0;

            do
            {
                compareReads(file, offset, partSize);

                offset += partSize;
            } while (offset + partSize < size);

            compareReads(file, offset, size - offset);
        }

        [Test]
        public void ReadOneByteFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 1);

            compareReads(file, 0, 1);
        }

        [Test]
        public void ReadWithBuffer()
        {
            long length = 100;
            long offset = 0;
            FileInfo file = RandomData.TempFile(tempDirPath, length*2);

            using (var stream = new ReadFilePartStream(file.FullName))
            {
                stream.Offset = offset;
                stream.PartLength = length;

                // attempt to read more data than available through the partstream
                int bytesRead = stream.Read(new byte[length*2], 0, (int) length*2);

                Assert.AreEqual(length, bytesRead);
            }
        }

        [Test]
        public void ReadZeroAtFileEnd()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 100);

            compareReads(file, 100, 0);
        }
    }
}