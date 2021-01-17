using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core
{
    public class IOPortBlock : IOPortBase
    {
        private readonly ushort _Size;

        public IOPortBlock(ushort aBaseAddress, ushort aSize) : base(aBaseAddress)
        {
            _Size = aSize;
        }

        public byte ReadByte(ushort offset)
        {
            return Read8((ushort)(Port + offset));
        }
        public void WriteByte(ushort offset, byte value)
        {
            Write8((ushort)(Port + offset), value);
        }

        public ushort ReadWord(ushort offset)
        {
            return Read16((ushort)(Port + offset));
        }
        public void WriteWord(ushort offset, ushort value)
        {
            Write16((ushort)(Port + offset), value);
        }

        public uint ReadDWord(ushort offset)
        {
            return Read32((ushort)(Port + offset));
        }
        public void WriteDWord(ushort offset, uint value)
        {
            Write32((ushort)(Port + offset), value);
        }
    }
}
