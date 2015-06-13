using System;
using System.Collections.Generic;
using System.IO;
using Coditate.Common.Util;

namespace Coditate.Common.IO
{
    /// <summary>
    /// Utility methods for supporting various IO operations.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Constant for the number of bytes per kilobyte.
        /// </summary>
        public const long BytesPerKilobyte = 1024;

        /// <summary>
        /// Constant for the number of bytes per megabyte.
        /// </summary>
        public const long BytesPerMegabyte = 1048576;

        /// <summary>
        /// Static buffer pool used by this class.
        /// </summary>
        public static readonly BufferPool Pool = new BufferPool();

        /// <summary>
        /// Determines whether a file path references a network resource.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// 	<c>true</c> if the path references a network resource; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNetworkPath(string path)
        {
            Arg.CheckNullOrEmpty("path", path);

            if (path.StartsWith(@"\\"))
            {
                return true;
            }
            string fullPath = Path.GetFullPath(path);
            var drive = new DriveInfo(fullPath);
            return drive.DriveType == DriveType.Network;
        }

        /// <summary>
        /// Determines whether this instance can read the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="error">Is set to the first exception caught while testing read access, 
        /// or null if no exception is caught.</param>
        /// <returns>
        /// 	<c>true</c> if the specified file can be read; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanRead(FileInfo file, out Exception error)
        {
            return CanRead(file, false, out error);
        }

        /// <summary>
        /// Determines whether this instance can read the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        /// 	<c>true</c> if the specified file can be read; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">Rethrows any exception thrown while checking read access.</exception>
        public static bool CanRead(FileInfo file)
        {
            Exception error;
            return CanRead(file, true, out error);
        }

        private static bool CanRead(FileInfo file, bool throwError, out Exception error)
        {
            Arg.CheckNull("file", file);

            error = null;

            try
            {
                using (file.OpenRead())
                {
                }
            }
            catch (Exception ex)
            {
                error = ex;
                if (throwError)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Transfers data between streams using asynchronous IO.
        /// </summary>
        /// <param name="from">Stream to read from.</param>
        /// <param name="to">Stream to write to.</param>
        public static void TransferData(Stream from, Stream to)
        {
            int bytesToWrite = 0;
            var readBuffer = Pool.Get();
            var writeBuffer = Pool.Get();
            do
            {
                var tempBuffer = readBuffer;
                readBuffer = writeBuffer;
                writeBuffer = tempBuffer;
                var readResult = from.BeginRead(readBuffer, 0, readBuffer.Length, null, null);
                if (bytesToWrite > 0)
                {
                    var writeResult = to.BeginWrite(writeBuffer, 0, bytesToWrite, null, null);
                    to.EndWrite(writeResult);
                }
                bytesToWrite = from.EndRead(readResult);

            } while (bytesToWrite > 0);

            Pool.Restore(readBuffer);
            Pool.Restore(writeBuffer);
        }

        /// <summary>
        /// Ensures that the specified directory path exists and that files on that path
        /// can be created and deleted.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <exception cref="Exception">Rethrows any exception thrown while ensuring write access.</exception>
        public static void EnsureWriteAccess(DirectoryInfo directoryInfo)
        {
            Exception error;
            EnsureWriteAccess(directoryInfo, true, out error);
        }

        /// <summary>
        /// Ensures that the specified directory path exists and that files on that path
        /// can be created and deleted.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <param name="error">Is set to the first exception caught while testing write access, 
        /// or null if no exception is caught.</param>
        public static void EnsureWriteAccess(DirectoryInfo directoryInfo, out Exception error)
        {
            EnsureWriteAccess(directoryInfo, false, out error);
        }

        private static void EnsureWriteAccess(DirectoryInfo directoryInfo, bool throwError,
                                              out Exception error)
        {
            Arg.CheckNull("directoryInfo", directoryInfo);

            error = null;

            directoryInfo.Refresh();
            var testFile =
                new FileInfo(Path.Combine(directoryInfo.FullName, Path.GetRandomFileName()));
            try
            {
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                using (FileStream stream = testFile.Create())
                {
                }
                testFile.Delete();
            }
            catch (Exception ex)
            {
                error = ex;
                if (throwError)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Closes the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public static void Close(TextReader reader)
        {
            if (reader != null)
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Closes the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static void Close(Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Closes the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public static void Close(TextWriter writer)
        {
            if (writer != null)
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        public static void Delete(FileInfo file)
        {
            if (file == null)
            {
                return;
            }
            file.Refresh();
            if (file.Exists)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="recursive">if set to <c>true</c> delete subdirectories.</param>
        public static void Delete(DirectoryInfo directory, bool recursive)
        {
            if (directory == null)
            {
                return;
            }
            directory.Refresh();
            if (directory.Exists)
            {
                directory.Delete(recursive);
            }
        }

        /// <summary>
        /// Gets a unique file based on a specified file name.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static FileInfo GetUniqueFile(string filePath)
        {
            var file = new FileInfo(filePath);
            FileInfo newFile = GetUniqueFile(file.DirectoryName, file.Name);
            return newFile;
        }

        /// <summary>
        /// Gets a unique directory based on a specified directory name.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public static DirectoryInfo GetUniqueDirectory(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            string uniquePath = directoryPath;
            int count = 0;
            while (Directory.Exists(uniquePath))
            {
                uniquePath = Path.Combine(directory.Parent.FullName, directory.Name + "_" + count++.ToString("00"));
            }
            return new DirectoryInfo(uniquePath);
        }

        /// <summary>
        /// Gets a unique file based on a specified file name.
        /// </summary>
        /// <param name="directory">The directory path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static FileInfo GetUniqueFile(string directory, string fileName)
        {
            string uniquePath = Path.Combine(directory, fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int count = 0;
            while (File.Exists(uniquePath))
            {
                uniquePath = Path.Combine(directory, name + "_" + count++.ToString("00") + ext);
            }
            return new FileInfo(uniquePath);
        }

        /// <summary>
        /// Checks if two strings represent the same actual file path, resolving partial paths, 
        /// casing differences, etc. and handling null or empty values.
        /// </summary>
        /// <param name="firstPath">The first path.</param>
        /// <param name="secondPath">The second path.</param>
        /// <returns>true if the paths are same, false if they are different or if either 
        /// path string is null or empty</returns>
        public static bool AreSamePath(string firstPath, string secondPath)
        {
            if (string.IsNullOrEmpty(firstPath) || string.IsNullOrEmpty(secondPath))
            {
                return false;
            }

            firstPath = Path.GetFullPath(firstPath);
            secondPath = Path.GetFullPath(secondPath);

            return string.Equals(firstPath, secondPath, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Copies the contents of a stream to a temp file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The temp file</returns>
        public static FileInfo CopyToTempFile(Stream stream)
        {
            var tempFile = new FileInfo(Path.GetTempFileName());
            var buffer = new byte[1000];
            int index = 0;
            int read = 0;

            using (FileStream fileStream = tempFile.OpenWrite())
            {
                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, read);
                    index += read;
                } while (read > 0);
            }
            return tempFile;
        }
    }
}