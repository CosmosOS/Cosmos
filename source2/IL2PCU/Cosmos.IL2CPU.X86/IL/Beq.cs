using System;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Beq)]
	public class Beq: ILOp {

    public Beq(Cosmos.IL2CPU.Assembler aAsmblr) : base(aAsmblr) {
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
			var xStackContent = OldAsmblr.StackContents.Pop();
			OldAsmblr.StackContents.Pop();
			if (xStackContent.Size > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			string BaseLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			if (xStackContent.Size <= 4)
			{
				new CPU.Pop { DestinationReg = CPU.Registers.EAX };
				new CPU.Pop { DestinationReg = CPU.Registers.EBX };
				new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EBX };
				new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };
        new CPU.Jump { DestinationLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value };
				new Label(LabelFalse);
				//new CPUx86.Noop();
				//new CPUx86.Jump(LabelFalse);
				//new CPU.Label(LabelTrue);
				//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
				//new CPUx86.Jump(TargetLabel);
				//new CPU.Label(LabelFalse);
				//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
			}
			else
			{
				new CPU.Pop { DestinationReg = CPU.Registers.EAX };
				new CPU.Pop { DestinationReg = CPU.Registers.EBX };
				new CPU.Pop { DestinationReg = CPU.Registers.ECX };
				new CPU.Pop { DestinationReg = CPU.Registers.EDX };
				new CPU.Xor { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.ECX };
				new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
				new CPU.Xor { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EDX };
				new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
        new CPU.Jump { DestinationLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value };
				new Label(LabelFalse);
				//new CPUx86.Noop();
			}
		}

    #region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Beq)]
		// 	public class Beq: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		// 		public Beq(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 			CurInstructionLabel = GetInstructionLabel(aReader);
		// 		}
		// 		public override void DoAssemble() {
		// 			var xStackContent = Assembler.StackContents.Pop();
		// 			Assembler.StackContents.Pop();
		// 			if (xStackContent.Size > 8) {
		// 				throw new Exception("StackSize>8 not supported");
		// 			}
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			if (xStackContent.Size <= 4)
		// 			{
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBX };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		// 				//new CPUx86.Noop();
		// 				//new CPUx86.Jump(LabelFalse);
		// 				//new CPU.Label(LabelTrue);
		// 				//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
		// 				//new CPUx86.Jump(TargetLabel);
		// 				//new CPU.Label(LabelFalse);
		// 				//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
		// 			} else
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
		//                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EDX };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		// 				//new CPUx86.Noop();
		// 			}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
