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
        /*XS.Pushfd();
        new CPUx86.Pop("eax");
        new CPUx86.Move("ecx", "eax");

        new CPUx86.Xor("eax", "200000h");
        new CPUx86.Push("eax");
        XS.Popfd();

        XS.Pushfd();
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
        XS.Popfd();

        new CPUx86.Push("eax");*/
      XS.Push(0);
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
      XS.Mov(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
      XS.CpuId();
      XS.Mov(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0x18);
      XS.Mov(XSRegisters.EDI, XSRegisters.EDX, destinationIsIndirect: true);
      XS.Mov(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0x14);
      XS.Mov(XSRegisters.EDI, XSRegisters.ECX, destinationIsIndirect: true);
      XS.Mov(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0x10);
      XS.Mov(XSRegisters.EDI, XSRegisters.EBX, destinationIsIndirect: true);
      XS.Mov(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0xC);
      XS.Mov(XSRegisters.EDI, XSRegisters.EAX, destinationIsIndirect: true);
      XS.Sti();
    }
  }
}
