using System;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Collections.Generic;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class CreateGDT: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      string xFieldName = "_NATIVE_GDT_Contents";
      string xFieldData // Null Segment
          = "0,0,0,0,0,0,0,0" // Code Segment
            + ", 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0" // Data Segment
            + ", 0xFF,0xFF,0,0,0,0x93,0xCF,0";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "db", xFieldData)));
      //xFieldName = "_NATIVE_GDT_Pointer";
      ////xFieldData = "0x17, (_NATIVE_GDT_Contents and 0xFFFF), (_NATIVE_GDT_Contents shr 16)";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "dw", "0x37,0,0")));

      aAssembler.DataMembers.Add(new CPUAll.DataMember(xFieldName, new byte[] {0,0,0,0,0,0,0,0 // Code Segment
		          , 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0 // Data Segment
		          , 0xFF,0xFF,0,0,0,0x93,0xCF,0}));
      aAssembler.DataMembers.Add(new CPUAll.DataMember("_NATIVE_GDT_Pointer", new ushort[] { 0x17, 0, 0 }));

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

    public override void AssembleNew(object aAssembler) {
      string xFieldName = "_NATIVE_GDT_Contents";
      string xFieldData // Null Segment
          = "0,0,0,0,0,0,0,0" // Code Segment
            + ", 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0" // Data Segment
            + ", 0xFF,0xFF,0,0,0,0x93,0xCF,0";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "db", xFieldData)));
      //xFieldName = "_NATIVE_GDT_Pointer";
      ////xFieldData = "0x17, (_NATIVE_GDT_Contents and 0xFFFF), (_NATIVE_GDT_Contents shr 16)";
      //aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember> (aAssembler.CurrentGroup,new DataMember(xFieldName, "dw", "0x37,0,0")));
      var xAssembler = (CosAssembler)aAssembler;
      xAssembler.DataMembers.Add(new CosCPUAll.DataMember(xFieldName, new byte[] {0,0,0,0,0,0,0,0 // Code Segment
		          , 0xFF, 0xFF, 0, 0, 0, 0x99, 0xCF, 0 // Data Segment
		          , 0xFF,0xFF,0,0,0,0x93,0xCF,0}));
      xAssembler.DataMembers.Add(new CosCPUAll.DataMember("_NATIVE_GDT_Pointer", new ushort[] { 0x17, 0, 0 }));

      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceRef = CosCPUAll.ElementReference.New("_NATIVE_GDT_Pointer") };
      new CosCPUx86.Move { DestinationRef = CosCPUAll.ElementReference.New("_NATIVE_GDT_Pointer"), DestinationIsIndirect = true, DestinationDisplacement = 2, SourceRef = CosCPUAll.ElementReference.New("_NATIVE_GDT_Contents") };

      new CosCPUAll.Label(".RegisterGDT");
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceRef = CosCPUAll.ElementReference.New("_NATIVE_GDT_Pointer") };
      new CosCPUx86.Lgdt { DestinationReg = CosCPUx86.Registers.EAX, DestinationIsIndirect = true };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.AX, SourceValue = 0x10 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.DS, SourceReg = CosCPUx86.Registers.AX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.ES, SourceReg = CosCPUx86.Registers.AX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.FS, SourceReg = CosCPUx86.Registers.AX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.GS, SourceReg = CosCPUx86.Registers.AX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.SS, SourceReg = CosCPUx86.Registers.AX };
      // Force reload of code segement
      new CosCPUx86.JumpToSegment { Segment = 8, DestinationLabel = "flush__GDT__table" };
      new CosCPUAll.Label("flush__GDT__table");
    }
  }
}
