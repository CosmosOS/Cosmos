using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldloca)]
  public class Ldloca: ILOp {
    public Ldloca(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpVar = (OpVar)aOpCode;
      var xAddress = GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      if (aMethod.MethodBase.Name == "Init" && aMethod.MethodBase.DeclaringType.Name == "Program")
      {
          Console.Write("");
      }
      xAddress += (GetStackCountForLocal(aMethod, aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value]) - 1) * 4;

      // xAddress contains full size of locals, excluding the actual local
      new CPUx86.Mov {
        DestinationReg = CPUx86.Registers.EAX,
        SourceReg = CPUx86.Registers.EBP
      };
      new CPUx86.Sub {
        DestinationReg = CPUx86.Registers.EAX,
        SourceValue = xAddress
      };
      new CPUx86.Push {
        DestinationReg = CPUx86.Registers.EAX,
      };
    }
  }
}