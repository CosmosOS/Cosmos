using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class VGA: IOGroup
    {
        public readonly IOPortWrite AttributeController_Index = new IOPortWrite(0x3C0);
        public readonly IOPortWrite AttributeController_Write = new IOPortWrite(0x3C0);
        public readonly IOPortRead AttributeController_Read = new IOPortRead(0x3C1);
        public readonly IOPortWrite MiscellaneousOutput_Write = new IOPortWrite(0x3C2);
        public readonly IOPortWrite Sequencer_Index = new IOPortWrite(0x3C4);
        public readonly IOPort Sequencer_Data = new IOPort(0x3C5);
        public readonly IOPortRead DACIndex_Read = new IOPortRead(0x3C7);
        public readonly IOPortWrite DACIndex_Write = new IOPortWrite(0x3C8);
        public readonly IOPortWrite DAC_Data = new IOPortWrite(0x3C9);
        public readonly IOPortWrite GraphicsController_Index = new IOPortWrite(0x3CE);
        public readonly IOPort GraphicsController_Data = new IOPort(0x3CF);
        public readonly IOPortWrite CRTController_Index = new IOPortWrite(0x3D4);
        public readonly IOPort CRTController_Data = new IOPort(0x3D5);
        public readonly IOPortRead Instat_Read = new IOPortRead(0x3DA);

        /// <summary>
        /// 64KB at 0xA0000
        /// </summary>
        public readonly MemoryBlock08 VGAMemoryBlock = new MemoryBlock08(0xA0000, 1024 * 64);

        /// <summary>
        /// 32KB at 0xB0000
        /// </summary>
        public readonly MemoryBlock08 MonochromeTextMemoryBlock = new MemoryBlock08(0xB0000, 1024 * 32);

        /// <summary>
        /// 32KB at 0xB8000
        /// </summary>
        public readonly MemoryBlock08 CGATextMemoryBlock = new MemoryBlock08(0xB8000, 1024 * 32);
    }
}
