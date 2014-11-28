using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(Cosmos.Core.IOPortBase))]
    public class IOPortImpl
    {

        #region Write8
        private class Write8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                //TODO: This is a lot of work to write to a single port.
                // We need to have some kind of inline ASM option that can
                // emit a single out instruction
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x0C, SourceIsIndirect = true };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x08, SourceIsIndirect = true };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.AL };
            }
        }
        [PlugMethod(Assembler=typeof(Write8Assembler))]
        public static void Write8(UInt16 aPort, byte aData) { }
        #endregion

        #region Write16
        private class Write16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.AX };
            }
        }
        [PlugMethod(Assembler = typeof(Write16Assembler))]
        public static void Write16(UInt16 aPort, UInt16 aData) { }
        #endregion

        #region Write32
        private class Write32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(Write32Assembler))]
        public static void Write32(UInt16 aPort, UInt32 aData) { }
        #endregion

        #region Read8
        private class Read8Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                //TODO: Do we need to clear rest of EAX first?
                //    MTW: technically not, as in other places, it _should_ be working with AL too..
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.IN { DestinationReg = CPUx86.Registers.AL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(Read8Assembler))]
        public static byte Read8(UInt16 aPort) { return 0; }
        #endregion

        #region Read16
        private class Read16Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.IN { DestinationReg = CPUx86.Registers.AX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(Read16Assembler))]
        public static UInt16 Read16(UInt16 aPort) { return 0; }
        #endregion

        #region Read32
        private class Read32Assembler : AssemblerMethod
        {
            public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.IN { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(Read32Assembler))]
        public static UInt32 Read32(UInt16 aPort) { return 0; }
        #endregion

    }
}
