using Cosmos.Core.Memory;
using Cosmos.Core;

namespace Cosmos.HAL.Drivers.Video
{
    public unsafe class VideoDriver
    {
        public VideoDriver(uint aWidth, uint aHeight)
        {
            Height = aHeight;
            Width = aWidth;
        }

        #region Methods

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

        public uint Height
        {
            get => _Width;
            set
            {
                if (_Width != 0)
                {
                    Buffer = (uint*)Heap.Alloc(Width * value * 4);
                    _Height = value;
                }
            }
        }
        public uint Width
        {
            get => _Width;
            set
            {
                if (_Height != 0)
                {
                    Buffer = (uint*)Heap.Alloc(value * Height * 4);
                    _Width = value;
                }
            }
        }

        public uint* Buffer;
        private uint _Height;
        private uint _Width;

        #endregion
    }
}
