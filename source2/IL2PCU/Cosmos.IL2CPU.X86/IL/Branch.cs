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
  public class Branch: ILOp {

    public Branch(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xStackContent = Assembler.Stack.Pop();
      var xIsSingleCompare = true;
      switch (aOpCode.OpCode) {
        case ILOpCode.Code.Beq:
        case ILOpCode.Code.Bge:
        case ILOpCode.Code.Bgt:
        case ILOpCode.Code.Bge_Un:
        case ILOpCode.Code.Bgt_Un:
        case ILOpCode.Code.Ble:
        case ILOpCode.Code.Ble_Un:
        case ILOpCode.Code.Bne_Un:
        case ILOpCode.Code.Blt:
        case ILOpCode.Code.Blt_Un:
          Assembler.Stack.Pop();
          xIsSingleCompare = false;
          break;
      }

      if (xStackContent.Size > 8) {
        throw new Exception("StackSize > 8 not supported");
      }

      CPU.ConditionalTestEnum xTestOp;
      // all conditions are inverted here?
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
      if (!xIsSingleCompare) {
        if (xStackContent.Size <= 4) {
          new CPU.Pop { DestinationReg = CPU.Registers.EAX };
          new CPU.Pop { DestinationReg = CPU.Registers.EBX };
          new CPU.Compare { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EAX };
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
      } else {
        // todo: improve code clarity
        if (xStackContent.Size > 4) {
          throw new Exception("Simple branches are not supported yet on operands > 4 bytes!");
        }
        new CPU.Pop { DestinationReg = CPU.Registers.EAX };
        if (xTestOp == ConditionalTestEnum.Zero) {
          new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceValue = 0 };
          new CPU.ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = AssemblerNasm.TmpBranchLabel(aMethod, aOpCode) };
        } else if (xTestOp == ConditionalTestEnum.NotZero) {
          new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceValue = 0 };
          new CPU.ConditionalJump { Condition = ConditionalTestEnum.NotEqual, DestinationLabel = AssemblerNasm.TmpBranchLabel(aMethod, aOpCode) };
        } else {
          throw new NotSupportedException("Situation not supported yet!");
        }
      }
    }

  }
}
