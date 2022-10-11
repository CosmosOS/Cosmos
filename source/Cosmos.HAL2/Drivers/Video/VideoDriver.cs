using Cosmos.Core.Memory;
using Cosmos.Core;
using System;

namespace Cosmos.HAL.Drivers.Video
{
    public unsafe class VideoDriver
    {
        /// <summary>
        /// Create a new instance of the VideoDriver base class.
        /// Used for video drivers to have fast memory access in a uniform class.
        /// </summary>
        /// <param name="aWidth">The width for the buffer.</param>
        /// <param name="aHeight">The height for the buffer.</param>
        /// <param name="aDepth">The depth of the buffer (bpp)</param>
        public VideoDriver(uint aWidth, uint aHeight, byte aDepth)
        {
            Height = aHeight;
            Width = aWidth;
            Depth = aDepth;
        }

        #region Methods

        // These are methods that need to be overridden by a base class.
        public virtual void Display()
        {
            throw new NotImplementedException();
        }
        public virtual void Disable()
        {
            throw new NotImplementedException();
        }
        public virtual void Update()
        {
            throw new NotImplementedException();
        }

        public void Set(uint aIndex, ushort aValue)
        {
            ((ushort*)Buffer)[aIndex] = aValue;
        }
        public void Set(uint aIndex, uint aValue)
        {
            Buffer[aIndex] = aValue;
        }
        public void Set(uint aIndex, byte aValue)
        {
            ((byte*)Buffer)[aIndex] = aValue;
        }
        public uint Get(uint aIndex)
        {
            return Buffer[aIndex];
        }

        public void Copy(uint* aAddress)
        {
            MemoryOperations.Copy(aAddress, Buffer, (int)(Width * Height * 4));
        }
        public void Fill(uint aStart, uint aEnd, uint aValue)
        {
            MemoryOperations.Fill(Buffer + aStart, aValue, (int)(aEnd - aStart));
        }
        public void Fill(uint aValue)
        {
            MemoryOperations.Fill(Buffer, aValue, (int)(Width * Height * 4));
        }

        #endregion

        #region Fields

        /// <summary>
        /// The height of the buffer.
        /// </summary>
        public virtual uint Height
        {
            get => _Width;
            set
            {
                if (_Width != 0 && _Depth != 0)
                {
                    Buffer = (uint*)Heap.Alloc(Width * (_Height = value) * Depth);
                }
            }
        }
        /// <summary>
        /// The width of the buffer.
        /// </summary>
        public virtual uint Width
        {
            get => _Width;
            set
            {
                if (_Height != 0 && _Depth != 0)
                {
                    Buffer = (uint*)Heap.Alloc((_Width = value) * Height * Depth);
                }
            }
        }
        /// <summary>
        /// The color depth of the buffer. (bits per pixel)
        ///  4 = 32 bit
        ///  3 = 24 bit
        ///  2 = 16 bit
        ///  1 = 8 bit
        /// </summary>
        public virtual byte Depth
        {
            get => _Depth;
            set
            {
                if (_Width != 0 && _Height != 0)
                {
                    Buffer = (uint*)Heap.Alloc(Width * Height * (_Depth = value));
                }
            }
        }

        public uint* Buffer;
        private uint _Height;
        private uint _Width;
        private byte _Depth;

        #endregion
    }
}
