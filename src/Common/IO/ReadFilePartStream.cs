using System;
using System.IO;
using Coditate.Common.Util;

namespace Coditate.Common.IO
{
    /// <summary>
    /// A stream for reading parts of files.
    /// </summary>
    /// <remarks>
    /// Passes through all operations to the underlying file stream. <see cref="Read"/> operations
    /// behave as though the underlying file begins at <see cref="Offset"/> and
    /// ends at <see cref="Offset"/> plus <see cref="PartLength"/>.
    /// 
    /// If offset and 
    /// </remarks>
    public class ReadFilePartStream : FileStream
    {
        private long offset;
        private long partLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadFilePartStream"/> class
        /// with the specified path and opens the underlying file stream for reading only.
        /// </summary>
        /// <param name="path">The path.</param>
        public ReadFilePartStream(string path) : base(path, FileMode.Open, FileAccess.Read)
        {
        }

        /// <summary>
        /// Gets or sets the offset from which to start reading the underlying file.
        /// </summary>
        /// <value>The offset.</value>
        public long Offset
        {
            get { return offset; }
            set
            {
                Arg.CheckInRange("Offset", value, 0, base.Length - PartLength);

                offset = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the file part to read.
        /// </summary>
        /// <value>The length of the part.</value>
        public long PartLength
        {
            get { return partLength; }
            set
            {
                Arg.CheckInRange("PartLength", value, 0, base.Length - Offset);

                partLength = value;
            }
        }

        /// <summary>
        /// Reads from the underlying file stream, beginning at <see cref="Offset"/>
        /// and ending at <see cref="Offset"/> plus <see cref="PartLength"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>The number of bytes read</returns>
        public override int Read(byte[] array, int offset, int count)
        {
            EnsureStartPosition();

            if (BytesAvailable() <= 0)
            {
                return 0;
            }
            count = (int) Math.Min(count, BytesAvailable());

            return base.Read(array, offset, count);
        }

        /// <summary>
        /// Reads next byte from the underlying file stream, beginning at <see cref="Offset"/>
        /// and ending at <see cref="Offset"/> plus <see cref="PartLength"/>.
        /// </summary>
        /// <returns>The next byte or -1 if at end of stream</returns>
        public override int ReadByte()
        {
            EnsureStartPosition();

            if (BytesAvailable() <= 0)
            {
                return -1;
            }

            return base.ReadByte();
        }

        /// <summary>
        /// Gets the stream length, which is the same as <see cref="PartLength"/>
        /// </summary>
        public override long Length
        {
            get { return PartLength; }
        }

        private void EnsureStartPosition()
        {
            if (base.Position < offset)
            {
                base.Seek(offset, SeekOrigin.Begin);
            }
        }

        private long BytesAvailable()
        {
            long bytesAvailable = (Offset + PartLength) - base.Position;
            bytesAvailable = Math.Max(0, bytesAvailable);
            return bytesAvailable;
        }
    }
}