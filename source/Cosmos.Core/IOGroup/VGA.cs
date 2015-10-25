using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Core.IOGroup
{
    public class TextScreen : IOGroup {
        public readonly MemoryBlock Memory = new MemoryBlock(0xB8000, 80 * 25 * 2);
        // These should probably move to a VGA class later, or this class should be remade into a VGA class
        public readonly IOPort MiscOutput = new IOPort(0x03C2);
        public readonly IOPort Idx1 = new IOPort(0x03C4);
        public readonly IOPort Data1 = new IOPort(0x03C5);
        public readonly IOPort Idx2 = new IOPort(0x03CE);
        public readonly IOPort Data2 = new IOPort(0x03CF);
        public readonly IOPort Idx3 = new IOPort(0x03D4);
        public readonly IOPort Data3 = new IOPort(0x03D5);
    }
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
