using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Core.Plugs {
    [Plug(Target = typeof(Cosmos.Core.IOPortBase))]
    public class IOPortImpl {

        public sealed class IOWrite8Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                //TODO: This is a lot of work to write to a single port. We need to have some kind of inline ASM option that can emit a single out instruction
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x0C, SourceIsIndirect = true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x08, SourceIsIndirect = true };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.AL };
            }
        }
        [PlugMethod(Assembler = typeof(IOWrite8Asm))]
        public static void Write8(UInt16 aPort, byte aData) { }

        public sealed class IOWrite16Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.AX };
            }
        }
        [PlugMethod(Assembler = typeof(IOWrite16Asm))]
        public static void Write16(UInt16 aPort, UInt16 aData) { }

        public sealed class IOWrite32Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x0C };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Out { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(IOWrite32Asm))]
        public static void Write32(UInt16 aPort, UInt32 aData) { }

        public sealed class IORead8Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                //TODO: Also make an attribute that forces normal inlining fo a method
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                //TODO: Do we need to clear rest of EAX first?
                //    MTW: technically not, as in other places, it _should_ be working with AL too..
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.In { DestinationReg = CPUx86.Registers.AL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(IORead8Asm))]
        public static byte Read8(UInt16 aPort) {
            return 0;
        }

        public sealed class IORead16Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.In { DestinationReg = CPUx86.Registers.AX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(IORead16Asm))]
        public static UInt16 Read16(UInt16 aPort) {
            return 0;
        }

        public sealed class IORead32Asm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x08 };
                new CPUx86.In { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(IORead32Asm))]
        public static UInt32 Read32(UInt16 aPort) {
            return 0;
        }
    }
}
