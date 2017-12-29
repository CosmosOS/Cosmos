using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core.MemoryGroup
{
    public class AHCI
    {
        public MemoryBlock DataBlock;

        public AHCI(uint aSectorSize)
        {
            DataBlock = new Core.MemoryBlock(0x0046C000, aSectorSize * 1024);
            DataBlock.Fill(0);
        }
    }
}
