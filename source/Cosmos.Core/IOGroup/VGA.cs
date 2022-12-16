using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// VGA class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class VGA: IOGroup
    {
        /// <summary>
        /// Attribute controller index port.
        /// </summary>
        public readonly int AttributeController_Index = 0x3C0;
        /// <summary>
        /// Attribute controller write port.
        /// </summary>
        public readonly int AttributeController_Write = 0x3C0;
        /// <summary>
        /// Attribute controller read port.
        /// </summary>
        public readonly int AttributeController_Read = 0x3C1;
        /// <summary>
        /// Miscellaneous output write port.
        /// </summary>
        public readonly int MiscellaneousOutput_Write = 0x3C2;
        /// <summary>
        /// Sequencer index port.
        /// </summary>
        public readonly int Sequencer_Index = 0x3C4;
        /// <summary>
        /// Sequencer data port.
        /// </summary>
        public readonly int Sequencer_Data = 0x3C5;
        /// <summary>
        /// DAC index read port.
        /// </summary>
        public readonly int DACIndex_Read = 0x3C7;
        /// <summary>
        /// DAC index write port.
        /// </summary>
        public readonly int DACIndex_Write = 0x3C8;
        /// <summary>
        /// DAC data port.
        /// </summary>
        public readonly int DAC_Data = 0x3C9;
        /// <summary>
        /// Graphics controller index port.
        /// </summary>
        public readonly int GraphicsController_Index = 0x3CE;
        /// <summary>
        /// Graphics controller data port.
        /// </summary>
        public readonly int GraphicsController_Data = 0x3CF;
        /// <summary>
        /// CRT controller index port.
        /// </summary>
        public readonly int CRTController_Index = 0x3D4;
        /// <summary>
        /// CRT controller data port.
        /// </summary>
        public readonly int CRTController_Data = 0x3D5;
        /// <summary>
        /// Instant read port.
        /// </summary>
        public readonly int Instat_Read = 0x3DA;

        /// <summary>
        /// 128KB at 0xA0000
        /// </summary>
        public readonly MemoryBlock VGAMemoryBlock = new MemoryBlock(0xA0000, 1024 * 128);

        /// <summary>
        /// 32KB at 0xB0000
        /// </summary>
        public readonly MemoryBlock MonochromeTextMemoryBlock = new MemoryBlock(0xB0000, 1024 * 32);

        /// <summary>
        /// 32KB at 0xB8000
        /// </summary>
        public readonly MemoryBlock CGATextMemoryBlock = new MemoryBlock(0xB8000, 1024 * 32);
    }
}
