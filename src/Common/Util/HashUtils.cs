using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Coditate.Common.IO;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Generates full or striped hashes of data.
    /// </summary>
    /// <remarks>
    /// A hash is generated from "stripes" of data from the file rather than all the 
    /// data contained in the file. This allows extremely fast hashing of large (>1GB) video
    /// files in just a few milliseconds rather than the 10-20 seconds it would take 
    /// to hash the complete data. Because the file size is also incorporated it is extremely 
    /// unlikely that the same hash will be generated for two different video files.
    /// 
    /// Hashes are generated using the MD5 algorithm and the hash is hex encoded and returned as
    /// a string.
    /// </remarks>
    public static class HashUtils
    {
        /// <summary>
        /// Default number of bytes per "stripe" for striped hashing.
        /// </summary>
        public static int DefaultStripeSize = 128;
        /// <summary>
        /// Default number of bytes for striped hashing.
        /// </summary>
        public static int DefaultBytesToHash = DefaultStripeSize*800; // 100kb

        /// <summary>
        /// Generates a striped hash from <see cref="DefaultBytesToHash"/> bytes of the file data.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The hex encoded hash</returns>
        public static string GenStripedHash(FileInfo file)
        {
            Arg.CheckNull("file", file);

            // refresh in case we are passed a fileinfo with old data
            file.Refresh();

            int bytesToHash = (int) Math.Min(DefaultBytesToHash, file.Length);
            int stripeCount = Math.Max(1, bytesToHash/DefaultStripeSize);
            int stripeSize = DefaultStripeSize;

            if (file.Length <= DefaultStripeSize)
            {
                stripeCount = 1;
                stripeSize = (int) file.Length;
            }

            HashAlgorithm hashAlg = MD5.Create();
            using (Stream stream = file.OpenRead())
            {
                long nextStripeStart = 0;
                byte[] buffer = new byte[DefaultStripeSize];
                do
                {
                    TranformNextBlock(stream, nextStripeStart, buffer, hashAlg);
                    nextStripeStart = nextStripeStart + (stream.Length/stripeCount);
                } while (nextStripeStart < stream.Length);

                // Always include the last bytes of the file for the final stripe
                TranformNextBlock(stream, stream.Length - stripeSize, buffer, hashAlg);

                // close out the hash operation
                hashAlg.TransformFinalBlock(buffer, 0, 0);
            }

            string hash = ToHexString(hashAlg.Hash);
            return hash;
        }

        /// <summary>
        /// Generates a hash of the full file data.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>the hex encoded hash</returns>
        public static string GenHash(FileInfo file)
        {
            Arg.CheckNull("file", file);

            // refresh in case we are passed a fileinfo with old data
            file.Refresh();

            using (Stream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GenHash(stream);
            }
        }

        /// <summary>
        /// Generates a hash of a section of the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="offset">The data offset.</param>
        /// <param name="length">The data length.</param>
        /// <returns></returns>
        public static string GenHash(FileInfo file, long offset, long length)
        {
            using (ReadFilePartStream pfs = new ReadFilePartStream(file.FullName))
            {
                pfs.Offset = offset;
                pfs.PartLength = length;

                return GenHash(pfs);
            }
        }

        /// <summary>
        /// Generates a hash of the full data stream.
        /// </summary>
        /// <param name="stream">The data stream.</param>
        /// <returns>the hex encoded hash</returns>
        public static string GenHash(Stream stream)
        {
            Arg.CheckNull("stream", stream);

            HashAlgorithm hashAlg = MD5.Create();
            byte[] hashBytes = hashAlg.ComputeHash(stream);

            string hash = ToHexString(hashBytes);
            return hash;
        }

        /// <summary>
        /// Generates a hash of the specified string.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <returns>the hex encoded hash</returns>
        /// <remarks>
        /// The string is converted to bytes using <see cref="Encoding.UTF8"/>
        /// encoding.
        /// </remarks>
        public static string GenHash(string text)
        {
            Arg.CheckNullOrEmpty("text", text);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return GenHash(stream);
        }

        private static void TranformNextBlock(Stream stream, long streamOffset, byte[] buffer,
                                              HashAlgorithm hashAlg)
        {
            stream.Seek(streamOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            hashAlg.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
        }

        /// <summary>
        /// Converts an array of bytes into a hex string.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static string ToHexString(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder(buffer.Length*2);
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Froms the hex string.
        /// </summary>
        /// <param name="hexedBytes">The hexed bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">If the string is not an even length</exception>
        /// <exception cref="FormatException">If string contains non-hex characters</exception>
        public static byte[] FromHexString(string hexedBytes)
        {
            Arg.CheckNullOrEmpty("hexedBytes", hexedBytes);

            bool evenLength = hexedBytes.Length%2 == 0;
            Arg.CheckCondition("hexedBytes", evenLength, "Length must be an even number of characters");

            byte[] buffer = new byte[hexedBytes.Length/2];

            for (int k = 0; k < buffer.Length; k++)
            {
                string oneByte = hexedBytes.Substring(k*2, 2);
                buffer[k] = Byte.Parse(oneByte, NumberStyles.HexNumber);
            }

            return buffer;
        }
    }
}