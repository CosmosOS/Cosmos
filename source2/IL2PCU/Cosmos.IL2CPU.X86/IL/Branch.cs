using System;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Beq)]
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge)]
  public class Branch : ILOp {

    public Branch(Cosmos.IL2CPU.Assembler aAsmblr) : base(aAsmblr) {
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
			var xStackContent = OldAsmblr.StackContents.Pop();
			OldAsmblr.StackContents.Pop();
			if (xStackContent.Size > 8) {
				throw new Exception("StackSize > 8 not supported");
			}

      CPU.ConditionalTestEnum xTestOp;
      switch (aOpCode.OpCode) {
        case ILOpCode.Code.Beq:
          xTestOp = CPU.ConditionalTestEnum.NotZero;
          break;
        case ILOpCode.Code.Bge:
          xTestOp = CPU.ConditionalTestEnum.GreaterThanOrEqualTo;
          break;
        default:
          throw new Exception("Unknown OpCode for branch.");
          break;
      }

			string BaseLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value + "__";
			string LabelFalse = BaseLabel + "False";
			if (xStackContent.Size <= 4) {
				new CPU.Pop { DestinationReg = CPU.Registers.EAX };
				new CPU.Pop { DestinationReg = CPU.Registers.EBX };
				new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EBX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = LabelFalse };
			} else {
				new CPU.Pop { DestinationReg = CPU.Registers.EAX };
				new CPU.Pop { DestinationReg = CPU.Registers.EBX };
				new CPU.Pop { DestinationReg = CPU.Registers.ECX };
				new CPU.Pop { DestinationReg = CPU.Registers.EDX };
				new CPU.Xor { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.ECX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = LabelFalse };
				new CPU.Xor { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EDX };
        new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = LabelFalse };
			}
      new CPU.Jump { DestinationLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value };
      new Label(LabelFalse);
    }

	}
}
