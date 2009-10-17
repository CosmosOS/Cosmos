using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUAll = Cosmos.IL2CPU;
using CPUx86 = Cosmos.IL2CPU.X86;
using System.Collections.Generic;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class CreateGDT: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      string xFieldName = "_NATIVE_GDT_Contents";
      string xFieldData // Null Segment
          = "0,0,0,0,0,0,0,0" // Code Segment
            + ", 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0" // Data Segment
            + ", 0xFF,0xFF,0,0,0,0x93,0xCF,0";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "db", xFieldData)));
      //xFieldName = "_NATIVE_GDT_Pointer";
      ////xFieldData = "0x17, (_NATIVE_GDT_Contents and 0xFFFF), (_NATIVE_GDT_Contents shr 16)";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "dw", "0x37,0,0")));
      var xAssembler = (Assembler)aAssembler;
      xAssembler.DataMembers.Add(new CPUAll.DataMember(xFieldName, new byte[] {0,0,0,0,0,0,0,0 // Code Segment
		          , 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0 // Data Segment
		          , 0xFF,0xFF,0,0,0,0x93,0xCF,0}));
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
      // Force reload of code segement
      new CPUx86.JumpToSegment { Segment = 8, DestinationLabel = "flush__GDT__table" };
      new CPUAll.Label("flush__GDT__table");
    }
  }
}
