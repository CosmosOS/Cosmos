using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core.MemoryGroup
{
    /// <summary>
    /// AHCI class.
    /// </summary>
    public class AHCI
    {
        /// <summary>
        /// Data memory block.
        /// </summary>
        public MemoryBlock DataBlock;

        /// <summary>
        /// Initialize AHCI instance.
        /// </summary>
        /// <param name="aSectorSize">A sector size.</param>
        public AHCI(uint aSectorSize)
        {
            DataBlock = new Core.MemoryBlock(0x0046C000, aSectorSize * 1024);
            DataBlock.Fill(0);
        }
    }
}
