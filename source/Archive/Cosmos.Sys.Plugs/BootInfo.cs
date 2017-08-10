using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target=typeof(Sys.BootInfo))]
    public static class BootInfo
    {
        [PlugMethod(Assembler=typeof(Assemblers.GetVbeControlInfoAssembler))]
        public static uint GetVbeControlInfoAddr()
        {
            return UInt32.MaxValue;
        }

        [PlugMethod(Assembler = typeof(Assemblers.GetVbeModeInfoAssembler))]
        public static uint GetVbeModeInfoAddr()
        {
            return UInt32.MaxValue;
        }

        [PlugMethod(Assembler = typeof(Assemblers.GetVbeModeAssembler))]
        public static ushort GetVbeMode()
        {
            return ushort.MaxValue;
        }
    }
}
