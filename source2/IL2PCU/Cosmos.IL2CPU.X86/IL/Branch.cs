using System;
using Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Beq)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bgt)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ble)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Blt)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bne_Un)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge_Un)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bgt_Un)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ble_Un)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Blt_Un)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Brfalse)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Brtrue)]
  public class Branch : ILOp {

    public Branch(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xStackContent = Assembler.Stack.Pop();
      Assembler.Stack.Pop();
      if (xStackContent.Size > 8) {
        throw new Exception("StackSize > 8 not supported");
      }

      CPU.ConditionalTestEnum xTestOp;
      switch (aOpCode.OpCode) {
        case ILOpCode.Code.Beq:
          xTestOp = CPU.ConditionalTestEnum.Zero;
          break;
        case ILOpCode.Code.Bge:
          xTestOp = CPU.ConditionalTestEnum.GreaterThanOrEqualTo;
          break;
        case ILOpCode.Code.Bgt:
          xTestOp = CPU.ConditionalTestEnum.GreaterThan;
          break;
        case ILOpCode.Code.Ble:
          xTestOp = CPU.ConditionalTestEnum.LessThanOrEqualTo;
          break;
        case ILOpCode.Code.Blt:
          xTestOp = CPU.ConditionalTestEnum.LessThan;
          break;
        case ILOpCode.Code.Bne_Un:
          xTestOp = CPU.ConditionalTestEnum.NotEqual;
          break;
        case ILOpCode.Code.Bge_Un:
          xTestOp = CPU.ConditionalTestEnum.AboveOrEqual;
          break;
        case ILOpCode.Code.Bgt_Un:
          xTestOp = CPU.ConditionalTestEnum.Above;
          break;
        case ILOpCode.Code.Ble_Un:
          xTestOp = CPU.ConditionalTestEnum.BelowOrEqual;
          break;
        case ILOpCode.Code.Blt_Un:
          xTestOp = CPU.ConditionalTestEnum.Below;
          break;
        case ILOpCode.Code.Brfalse:
          xTestOp = CPU.ConditionalTestEnum.Zero;
          break;
        case ILOpCode.Code.Brtrue:
          xTestOp = CPU.ConditionalTestEnum.NotZero;
          break;
        default:
          throw new Exception("Unknown OpCode for conditional branch.");
      }

      if (xStackContent.Size <= 4) {
        new CPU.Pop { DestinationReg = CPU.Registers.EAX };
        new CPU.Pop { DestinationReg = CPU.Registers.EBX };
        new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EBX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel(aMethod, aOpCode) };
      } else {
        new CPU.Pop { DestinationReg = CPU.Registers.EAX };
        new CPU.Pop { DestinationReg = CPU.Registers.EBX };
        new CPU.Pop { DestinationReg = CPU.Registers.ECX };
        new CPU.Pop { DestinationReg = CPU.Registers.EDX };
        new CPU.Xor { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.ECX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel(aMethod, aOpCode) };
        new CPU.Xor { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EDX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel(aMethod, aOpCode) };
      }
    }

  }
}
