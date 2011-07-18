using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(Cosmos.Core.IOPortBase))]
    public class IOPortImpl
    {
        [Inline]
        public static void Write8(UInt16 aPort, byte aData)
        {
            //TODO: This is a lot of work to write to a single port.
            // We need to have some kind of inline ASM option that can
            // emit a single out instruction
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x0C, SourceIsIndirect = true };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x08, SourceIsIndirect = true };
            new CPUx86.Out { DestinationReg = CPUx86.Registers.AL };
        }

        [Inline]
        public static void Write16(UInt16 aPort, UInt16 aData)
        {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
            new CPUx86.Out { DestinationReg = CPUx86.Registers.AX };
        }

        [Inline]
        public static void Write32(UInt16 aPort, UInt32 aData) 
        {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
            new CPUx86.Out { DestinationReg = CPUx86.Registers.EAX };
        }

        [Inline]
        public static byte Read8(UInt16 aPort)
        {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
            //TODO: Do we need to clear rest of EAX first?
            //    MTW: technically not, as in other places, it _should_ be working with AL too..
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.IN { DestinationReg = CPUx86.Registers.AL };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            return 0;
        }

        [Inline]
        public static UInt16 Read16(UInt16 aPort)
        {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.IN { DestinationReg = CPUx86.Registers.AX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            return 0;
        }

        [Inline]
        public static UInt32 Read32(UInt16 aPort)
        {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
            new CPUx86.IN { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            return 0;
        }
    }
}
