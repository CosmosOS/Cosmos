using System;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class CPUIDSupport: AssemblerMethod {
    //; Method 'System.UInt32  Cosmos.Kernel.Plugs.CPU.HasCPUIDSupport()'
    //; Locals:
    //; 	(0) 0	4	ebp - 04h (Type = System.UInt32)
    //; Arguments:
    //; 	(none)
    //; 	ReturnSize: 4
    public override void Assemble(Assembler aAssembler) {

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

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.Push { DestinationValue = 0 };
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
    public override void Assemble(Assembler aAssembler) {
      new CPUx86.ClrInterruptFlag();
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
      new CPUx86.CpuId();
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x18 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x14 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ECX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x10 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
      new CPUx86.Sti();
    }

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.ClrInterruptFlag();
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
      new CosCPUx86.CpuId();
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x18 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CosCPUx86.Registers.EDX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x14 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CosCPUx86.Registers.ECX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x10 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CosCPUx86.Registers.EBX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, DestinationIsIndirect = true, SourceReg = CosCPUx86.Registers.EAX };
      new CosCPUx86.Sti();
    }
  }
}
