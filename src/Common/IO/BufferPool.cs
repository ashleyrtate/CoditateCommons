using System.Collections;
using System.Collections.Generic;

namespace Coditate.Common.IO
{
    /// <summary>
    /// A pool of byte array buffers for use with intensive IO operations.
    /// </summary>
    public class BufferPool
    {
        /// <summary>
        /// Default buffer length. Value is 80,000 (below LOH threshold).
        /// </summary>
        public const int DefaultBufferLength = 80000;

        private readonly LinkedList<byte[]> Buffers = new LinkedList<byte[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        public BufferPool()
        {
            BufferLength = DefaultBufferLength;
        }

        /// <summary>
        /// Gets or sets the length of the created buffers.
        /// </summary>
        /// <value>The length of the buffer.</value>
        public int BufferLength { get; set; }

        /// <summary>
        /// Gets a buffer from the pool, creating new buffers as necessary.
        /// </summary>
        /// <returns></returns>
        public byte[] Get()
        {
            byte[] buffer;
            lock (((ICollection) Buffers).SyncRoot)
            {
                if (Buffers.Count == 0)
                {
                    Restore(new byte[BufferLength]);
                }
                buffer = Buffers.First.Value;
                Buffers.RemoveFirst();
            }
            return buffer;
        }

        /// <summary>
        /// Restores a buffer to the pool.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void Restore(byte[] buffer)
        {
            lock (((ICollection) Buffers).SyncRoot)
            {
                Buffers.AddLast(buffer);
            }
        }
    }
}