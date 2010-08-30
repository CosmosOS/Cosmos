using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using System.Collections.Generic;

namespace Cosmos.Core.Plugs.Assemblers {
    public class CreateGDT : AssemblerMethod {
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            var xAssembler = (Assembler)aAssembler;
            xAssembler.DataMembers.Add(new CPUAll.DataMember("_NATIVE_GDT_Contents"
                , new byte[] 
                {0,0,0,0,0,0,0,0 // Null Segment
		        , 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0 // Code Segment
		        , 0xFF,0xFF,0,0,0,0x93,0xCF,0})); // Data Segment
            xAssembler.DataMembers.Add(new CPUAll.DataMember("_NATIVE_GDT_Pointer", new ushort[] { 0x17, 0, 0 }));

            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_GDT_Pointer") };
            new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("_NATIVE_GDT_Pointer"), DestinationIsIndirect = true, DestinationDisplacement = 2, SourceRef = CPUAll.ElementReference.New("_NATIVE_GDT_Contents") };

            new CPUAll.Label(".RegisterGDT");
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_GDT_Pointer") };
            new CPUx86.Lgdt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };

            new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceValue = 0x10 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.DS, SourceReg = CPUx86.Registers.AX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ES, SourceReg = CPUx86.Registers.AX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.FS, SourceReg = CPUx86.Registers.AX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.GS, SourceReg = CPUx86.Registers.AX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.SS, SourceReg = CPUx86.Registers.AX };

            // Force reload of code segment
            new CPUx86.JumpToSegment { Segment = 8, DestinationLabel = "flush__GDT__table" };
            new CPUAll.Label("flush__GDT__table");
        }
    }
}
