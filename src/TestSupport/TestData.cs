using System;
using System.IO;
using System.Text;
using Coditate.Common.Util;
using System.Collections;

namespace Coditate.TestSupport
{
    // todo: move these methods over to RandomData class and get rid of this one
    
    /// <summary>
    /// Class for generating various types of random test data.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Random number generater used by this class.
        /// </summary>
        public static readonly Random Rand = RandomData.Generator;

        /// <summary>
        /// Returns a random integer.
        /// </summary>
        /// <returns></returns>
        public static int RandomInt()
        {
            return Rand.Next();
        }

        /// <summary>
        /// Chooses a random collecton value.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static T RandomListValue<T>(ICollection collection)
        {
            int index = Rand.Next(0, collection.Count);
            int count = 0;
            foreach (T o in collection)
            {
                if (index == count)
                {
                    return o;
                }
                count++;
            }
            // should never reach this
            return default(T);
        }

        /// <summary>
        /// Generates a random double value.
        /// </summary>
        /// <returns></returns>
        public static double RandomDouble()
        {
            return Rand.NextDouble();
        }

        /// <summary>
        /// Generates a random ascii string with values between 
        /// Space (0x20) and Tilde (0x7E). No control characters are included.
        /// </summary>
        /// <param name="length">The length of the requested string.</param>
        /// <returns>The generated string</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the requested length is less than zero</exception>
        public static string RandomAsciiString(int length)
        {
            Arg.CheckInRange("length", length, 0, int.MaxValue);

            StringBuilder sb = new StringBuilder(length);
            for (int k = 0; k < length; k++)
            {
                // add a character between space and tilde (no control characters)
                sb.Append(RandomData.AsciiChar());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random string containing only ascii letters, numbers, and spaces.
        /// </summary>
        /// <param name="length">The length of the requested string.</param>
        /// <returns>The generated string</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the requested length is less than zero</exception>
        public static string RandomAlphaNumericString(int length)
        {
            return RandomAlphaNumericString(length, true);
        }

        /// <summary>
        /// Generates a random string containing only ascii letters, numbers, and (optionally) spaces.
        /// </summary>
        /// <param name="length">The length of the requested string.</param>
        /// <param name="includeSpaces">if set to <c>true</c> include spaces in returned string (never 2 consecutive).</param>
        /// <returns>The generated string</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the requested length is less than zero</exception>
        public static string RandomAlphaNumericString(int length, bool includeSpaces)
        {
            return RandomData.AlphaNumericString(length, includeSpaces);
        }

        /// <summary>
        /// Writes a temp file containing random bytes of data.
        /// </summary>
        /// <param name="directoryPath">The directory path on which to write the file.</param>
        /// <param name="size">The size of the file to write.</param>
        /// <returns>The new file</returns>
        public static FileInfo RandomTempFile(string directoryPath, long size)
        {
            // todo: add unit test for this method
            Arg.CheckNullOrEmpty("directoryPath", directoryPath);

            Directory.CreateDirectory(directoryPath);

            string path = Path.Combine(directoryPath, Path.GetRandomFileName());
            FileInfo file = new FileInfo(path);
            using (Stream stream = file.OpenWrite())
            {
                byte[] buffer = new byte[10000];
                int bytesWritten = 0;
                do
                {
                    Rand.NextBytes(buffer);
                    int bytesToWrite = (int) Math.Min(size - bytesWritten, buffer.Length);
                    stream.Write(buffer, 0, bytesToWrite);
                    bytesWritten += bytesToWrite;
                } while (bytesWritten < size);
            }

            file.Refresh();

            return file;
        }

        /// <summary>
        /// Chooses a random element from the specified enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomEnumValue<T>() where T : struct
        {
            Array values = Enum.GetValues(typeof (T));
            int index = Rand.Next(values.Length);
            return (T) values.GetValue(index);
        }

        /// <summary>
        /// Generates a random DateTime in the specified range.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns></returns>
        public static DateTime RandomDateTime(DateTime min, DateTime max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("min", "min must be less than max");
            }

            long diffSeconds = (max.Ticks - min.Ticks)/TimeSpan.TicksPerSecond;
            long adjustSeconds = (long) (Rand.NextDouble()*diffSeconds);

            return new DateTime(min.Ticks + adjustSeconds*TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <returns></returns>
        public static bool RandomBool()
        {
            return Rand.Next(0, 2) == 1;
        }

        /// <summary>
        /// Generates a random TimeSpan in the specifie range.
        /// </summary>
        /// <param name="minTicks">The min ticks.</param>
        /// <param name="maxTicks">The max ticks.</param>
        /// <returns></returns>
        public static TimeSpan RandomDuration(long minTicks, long maxTicks)
        {
            if (minTicks > maxTicks)
            {
                throw new ArgumentOutOfRangeException("minTicks",
                                                      "minTicks must be less than maxTicks");
            }

            int diffSeconds = (int) ((maxTicks - minTicks)/TimeSpan.TicksPerSecond);
            int adjustSeconds = Rand.Next(diffSeconds + 1);

            return new TimeSpan(minTicks + adjustSeconds*TimeSpan.TicksPerSecond);
        }
    }
}