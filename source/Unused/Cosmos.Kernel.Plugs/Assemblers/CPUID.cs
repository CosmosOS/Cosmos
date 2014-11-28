using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class CPUIDSupport: AssemblerMethod {
    //; Method 'System.UInt32  Cosmos.Kernel.Plugs.CPU.HasCPUIDSupport()'
    //; Locals:
    //; 	(0) 0	4	ebp - 04h (Type = System.UInt32)
    //; Arguments:
    //; 	(none)
    //; 	ReturnSize: 4
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
        /*new CPUx86.Pushfd();
        new CPUx86.Pop("eax");
        new CPUx86.Move("ecx", "eax");

        new CPUx86.Xor("eax", "200000h");
        new CPUx86.Push("eax");
        new CPUx86.Popfd();

        new CPUx86.Pushfd();
        new CPUx86.Pop("ebx");
        new CPUx86.Xor("eax", "ebx");
        new CPUx86.And("eax", "200000h");
        new CPUx86.JumpIfZero(".not");

        new CPUx86.Move("eax", "1");
        new CPUx86.Jump(".return");

        new CPUAll.Label(".not");
        new CPUx86.Xor("eax", "eax");

        new CPUAll.Label(".return");
        new CPUx86.Push("ecx");
        new CPUx86.Popfd();

        new CPUx86.Push("eax");*/
      new CPUx86.Push { DestinationValue = 0 };
    }
  }
  public class GetCPUIDInternal: AssemblerMethod {
    //        ; (No Type Info available)
    //; Method 'System.Void  Cosmos.Kernel.Plugs.CPU.GetCPUId(System.UInt32&, System.UInt32&, System.UInt32&, System.UInt32&, System.UInt32)'
    //; Locals:
    //; 	(none)
    //; Arguments:
    //; 	(0) 16	4	ebp + 018h (Type = System.UInt32&)
    //; 	(1) 12	4	ebp + 014h (Type = System.UInt32&)
    //; 	(2) 8	4	ebp + 010h (Type = System.UInt32&)
    //; 	(3) 4	4	ebp + 0Ch (Type = System.UInt32&)
    //; 	(4) 0	4	ebp + 08h (Type = System.UInt32)
    //; 	ReturnSize: 0
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      new CPUx86.ClrInterruptFlag();
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
      new CPUx86.CpuId();
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x18 };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x14 };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ECX };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x10 };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
      new CPUx86.Sti();
    }
  }
}
