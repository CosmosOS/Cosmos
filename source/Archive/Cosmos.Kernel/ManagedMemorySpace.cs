using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel
{
    /// <summary>
    /// Provides Managed Memory allocation for drivers that need memory address access and memory address alignment
    /// </summary>
    public unsafe class ManagedMemorySpace : MemoryAddressSpace
    {
        private byte[] memory;

        /// <summary>
        /// Create a new buffer with the given size, not aligned
        /// </summary>
        /// <param name="size">Size of buffer</param>
        public ManagedMemorySpace(UInt32 size)
            : this(size, 1, false)
        { }

        /// <summary>
        /// Create a Managed buffer reference to an existing buffer
        /// <remarks>This can only be used for unaligned buffers.</remarks>
        /// </summary>
        /// <param name="buffer">Existing byte buffer</param>
        public ManagedMemorySpace(byte[] buffer)
            : base(0, (uint)buffer.Length)
        {
            fixed (byte* bodystart = &buffer[0])
            {
                this.Offset = (UInt32)bodystart;
            }
        }
        /// <summary>
        /// Create a new buffer with the given size, aligned on the byte boundary specified
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        public ManagedMemorySpace(UInt32 size, byte alignment)
            : this(size, alignment, true)
        { }

        /// <summary>
        /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        /// <param name="align">true if buffer should be aligned, false otherwise</param>
        public ManagedMemorySpace(UInt32 size, byte alignment, bool align)
            : base(0, size)
        {
            memory = new byte[size + alignment - 1];
            fixed (byte* bodystart = memory)
            {
                this.Offset = (UInt32)bodystart;
            }
            if (align == true)
            {
                while (this.Offset % alignment != 0)
                {
                    this.Offset++;
                }
            }
        }

        /// <summary>
        /// Get or set the byte at the given offset
        /// </summary>
        /// <param name="offset">Address Offset</param>
        /// <returns>Byte value at given offset</returns>
        public byte this[uint offset]
        {
            get { return this.Read8(offset); }
            set { this.Write8(offset, value); }
        }
    }
}
