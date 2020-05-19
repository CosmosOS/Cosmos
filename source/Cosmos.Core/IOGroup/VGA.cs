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
        public readonly IOPortWrite DACIndex_Read = new IOPortWrite(0x3C7);
        public readonly IOPortWrite DACIndex_Write = new IOPortWrite(0x3C8);
        public readonly IOPort DAC_Data = new IOPort(0x3C9);
        public readonly IOPortWrite GraphicsController_Index = new IOPortWrite(0x3CE);
        public readonly IOPort GraphicsController_Data = new IOPort(0x3CF);
        public readonly IOPortWrite CRTController_Index = new IOPortWrite(0x3D4);
        public readonly IOPort CRTController_Data = new IOPort(0x3D5);
        public readonly IOPortRead Instat_Read = new IOPortRead(0x3DA);

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
