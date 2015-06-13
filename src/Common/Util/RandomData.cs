using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Generates various types of random data.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class RandomData
    {
        /// <summary>
        /// Random number generater used by this class.
        /// </summary>
        public static readonly Random Generator = new Random();

        /// <summary>
        /// Generates a random string containing only ascii letters, numbers, and (optionally) spaces.
        /// </summary>
        /// <param name="length">The length of the requested string.</param>
        /// <param name="includeSpaces">if set to <c>true</c> include spaces in returned string (never 2 consecutive).</param>
        /// <returns>The generated string</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the requested length is less than zero</exception>
        public static string AlphaNumericString(int length, bool includeSpaces)
        {
            Arg.CheckInRange("length", length, 0, int.MaxValue);

            var sb = new StringBuilder(length);
            char lastC = '0';
            while (sb.Length < length)
            {
                // generate a random char and discard if a control character
                char c = AsciiChar();
                if ((c == ' ' && includeSpaces && c != lastC) || char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    lastC = c;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a list of random, unique, alphanumeric strings.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="minLength">Min string length.</param>
        /// <param name="maxLength">Max string length.</param>
        /// <param name="includeSpaces">if set to <c>true</c> include spaces.</param>
        /// <param name="lowercase">if set to <c>true</c> convert to lowercase.</param>
        /// <returns></returns>
        public static List<string> StringList(int count, int minLength, int maxLength, bool includeSpaces, bool lowercase)
        {
            Arg.CheckInRange("count", count, 0, int.MaxValue);
            Arg.CheckInRange("minLength", minLength, 1, int.MaxValue);
            Arg.CheckInRange("maxLength", maxLength, 1, int.MaxValue);
            Arg.CheckInRange("maxLength", maxLength, minLength, int.MaxValue);

            var values = new List<string>();
            do
            {
                int length = Generator.Next(minLength, maxLength);
                string value = AlphaNumericString(length, includeSpaces).Trim();
                if (lowercase)
                {
                    value = value.ToLowerInvariant();
                }
                if (value.Length > 0 && !values.Contains(value))
                {
                    values.Add(value);
                }
            } while (values.Count < count);
            return values;
        }

        /// <summary>
        /// Generates a random ascii string with values between 
        /// Space (0x20) and Tilde (0x7E). No control characters are included.
        /// </summary>
        /// <param name="length">The length of the requested string.</param>
        /// <returns>The generated string</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the requested length is less than zero</exception>
        public static string AsciiString(int length)
        {
            Arg.CheckInRange("length", length, 0, int.MaxValue);

            var sb = new StringBuilder(length);
            for (int k = 0; k < length; k++)
            {
                // add a character between space and tilde (no control characters)
                sb.Append(AsciiChar());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random string that may contain any possible characters.
        /// </summary>
        /// <param name="length">The desired string length.</param>
        /// <returns></returns>
        public static string String(int length)
        {
            var sb = new StringBuilder(length);
            while (sb.Length < length)
            {
                // generate a random char 
                var c = (char) Generator.Next(char.MaxValue);
                if (char.IsSurrogate(c))
                {
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a random byte array.
        /// </summary>
        /// <param name="count">The length of the requested array.</param>
        /// <returns></returns>
        public static byte[] Binary(int count)
        {
            var b = new byte[count];
            Generator.NextBytes(b);
            return b;
        }

        /// <summary>
        /// Returns a random integer.
        /// </summary>
        /// <returns></returns>
        public static int Int()
        {
            return Generator.Next();
        }

        /// <summary>
        /// Chooses a random collecton value.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static T ListValue<T>(ICollection collection)
        {
            int index = Generator.Next(0, collection.Count);
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
        public static double Double()
        {
            return Generator.NextDouble();
        }

        /// <summary>
        /// Writes a temp file containing random bytes of data.
        /// </summary>
        /// <param name="directoryPath">The directory path on which to write the file.</param>
        /// <param name="size">The size of the file to write.</param>
        /// <returns>The new file</returns>
        public static FileInfo TempFile(string directoryPath, long size)
        {
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
                    Generator.NextBytes(buffer);
                    int bytesToWrite = (int)Math.Min(size - bytesWritten, buffer.Length);
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
        public static T EnumValue<T>() where T : struct
        {
            Array values = Enum.GetValues(typeof(T));
            int index = Generator.Next(values.Length);
            return (T)values.GetValue(index);
        }

        /// <summary>
        /// Generates a random DateTime in the specified range.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns></returns>
        /// <remarks>
        /// The returned date time will have the same <see cref="DateTimeKind"/>
        /// as the min date parameter.
        /// </remarks>
        public static DateTime DateTime(DateTime min, DateTime max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("min", "min must be less than max");
            }

            long diffSeconds = (max.Ticks - min.Ticks) / TimeSpan.TicksPerSecond;
            long adjustSeconds = (long)(Generator.NextDouble() * diffSeconds);

            return new DateTime(min.Ticks + adjustSeconds * TimeSpan.TicksPerSecond, min.Kind);
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <returns></returns>
        public static bool Bool()
        {
            return Generator.Next(0, 2) == 1;
        }

        /// <summary>
        /// Generates a random string containing only numbers.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string NumericString(int length)
        {
            var sb = new StringBuilder(length);
            for (int k = 0; k < length; k++)
            {
                sb.Append((char) Generator.Next(0x30, 0x3A));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a random TimeSpan in the specified range.
        /// </summary>
        /// <param name="minTicks">The min ticks.</param>
        /// <param name="maxTicks">The max ticks.</param>
        /// <returns></returns>
        public static TimeSpan Duration(long minTicks, long maxTicks)
        {
            if (minTicks > maxTicks)
            {
                throw new ArgumentOutOfRangeException("minTicks",
                                                      "minTicks must be less than maxTicks");
            }

            int diffSeconds = (int)((maxTicks - minTicks) / TimeSpan.TicksPerSecond);
            int adjustSeconds = Generator.Next(diffSeconds + 1);

            return new TimeSpan(minTicks + adjustSeconds * TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// Generates a semi-random ascii character.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Space characters are generated 1/8 of the time.
        /// </remarks>
        public static char AsciiChar()
        {
            // explicitly returns space character 1 / 8 of the time
            if (Generator.Next(8) > 0)
            {
                return (char) Generator.Next(0x21, 0x7E);
            }
            else
            {
                return ' ';
            }
        }
    }
}