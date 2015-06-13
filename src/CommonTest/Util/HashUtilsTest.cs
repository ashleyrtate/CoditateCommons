using System;
using System.IO;
using Coditate.Common.IO;
using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class HashUtilsTest
    {
        private static int StandardFileSize = 1000000;
        private static int MaxFileSize = 10000000;

        private static readonly Random random = new Random();
        private string tempDirPath;

        [SetUp]
        public void Setup()
        {
            tempDirPath = Path.Combine(Path.GetTempPath(), "StripedHashGeneratorTest");
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
            Directory.CreateDirectory(tempDirPath);
        }

        [TearDown]
        public void TearDown()
        {
            IOUtils.Delete(new DirectoryInfo(tempDirPath), true);
        }

        [Test]
        public void SameFileFirstByteDifferent()
        {
            FileInfo file1 = RandomData.TempFile(tempDirPath, random.Next(StandardFileSize));
            FileInfo file2 = file1.CopyTo(file1.FullName + 2, true);

            using (Stream stream = file1.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                byte firstByte = (byte) stream.ReadByte();
                stream.Position = 0;
                stream.WriteByte(++firstByte);
            }

            string hash1 = HashUtils.GenStripedHash(file1);
            string hash2 = HashUtils.GenStripedHash(file2);

            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void SameFilePlusOneByte()
        {
            FileInfo file1 = RandomData.TempFile(tempDirPath, random.Next(StandardFileSize));
            FileInfo file2 = file1.CopyTo(file1.FullName + 2, true);

            using (Stream stream = file1.OpenWrite())
            {
                stream.Position = stream.Length;
                stream.WriteByte((byte) random.Next());
            }

            string hash1 = HashUtils.GenStripedHash(file1);
            string hash2 = HashUtils.GenStripedHash(file2);

            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void SameSizeSameEndBytesFilesAreDifferent()
        {
            byte[] buffer = new byte[random.Next(10000)];
            random.NextBytes(buffer);

            FileInfo file1 = RandomData.TempFile(tempDirPath, random.Next(StandardFileSize));
            FileInfo file2 = RandomData.TempFile(tempDirPath, random.Next(StandardFileSize));

            using (Stream stream1 = file1.OpenWrite(), stream2 = file2.OpenWrite())
            {
                stream1.Write(buffer, 0, buffer.Length);
                stream2.Write(buffer, 0, buffer.Length);
            }

            string hash1 = HashUtils.GenStripedHash(file1);
            string hash2 = HashUtils.GenStripedHash(file2);

            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void HashDefaultHashBytesSizeFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, HashUtils.DefaultBytesToHash);
            string hash1 = HashUtils.GenStripedHash(file);
            string hash2 = HashUtils.GenStripedHash(file);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void HashStripeSizedFiles()
        {
            for (int k = 0; k < 3; k++)
            {
                FileInfo file =
                    RandomData.TempFile(tempDirPath, HashUtils.DefaultStripeSize*k);
                string hash1 = HashUtils.GenStripedHash(file);
                string hash2 = HashUtils.GenStripedHash(file);

                Assert.AreEqual(hash1, hash2);
            }
        }

        [Test]
        public void HashOneByteFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, 1);
            string hash1 = HashUtils.GenStripedHash(file);
            string hash2 = HashUtils.GenStripedHash(file);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void FullHashLargeFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, MaxFileSize);
            string hash1 = HashUtils.GenHash(file);
            string hash2 = HashUtils.GenHash(file);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void HashLargeFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, MaxFileSize);
            string hash1 = HashUtils.GenStripedHash(file);
            string hash2 = HashUtils.GenStripedHash(file);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void HashRandomFile()
        {
            FileInfo file = RandomData.TempFile(tempDirPath, random.Next(StandardFileSize));
            string hash1 = HashUtils.GenStripedHash(file);
            string hash2 = HashUtils.GenStripedHash(file);

            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void HashEmptyFile()
        {
            FileInfo file1 = RandomData.TempFile(tempDirPath, 0);
            FileInfo file2 = RandomData.TempFile(tempDirPath, 0);
            string hash1 = HashUtils.GenStripedHash(file1);
            string hash2 = HashUtils.GenStripedHash(file2);

            // two empty files should hash to the same value
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void ToFromHexString()
        {
            byte[] buffer1 = new byte[20];
            random.NextBytes(buffer1);

            string hexed = HashUtils.ToHexString(buffer1);
            byte[] buffer2 = HashUtils.FromHexString(hexed);

            Assert.AreEqual(buffer1.Length, buffer2.Length);

            for (int k = 0; k < buffer1.Length; k++)
            {
                Assert.AreEqual(buffer1[k], buffer2[k]);
            }
        }
    }
}