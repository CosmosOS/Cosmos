using System;
using System.IO;
using System.Linq;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Throw)]
	public class Throw: Op {
		private MethodInformation mMethodInfo;
		private int mCurrentILOffset;
		public Throw(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mMethodInfo = aMethodInfo;
			mCurrentILOffset = (int)aReader.Position;
		}

		public static void Assemble(Assembler.Assembler aAssembler, MethodInformation aMethodInfo, int aCurrentILOffset) {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationRef = new CPU.ElementReference(CPU.DataMember.GetStaticFieldName(CPU.Assembler.CurrentExceptionRef)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
			Engine.QueueMethod(CPU.Assembler.CurrentExceptionOccurredRef);
            new CPUx86.Call { DestinationLabel = CPU.Label.GenerateLabelName(CPU.Assembler.CurrentExceptionOccurredRef) };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 3 };
			Call.EmitExceptionLogic(aAssembler, (uint)aCurrentILOffset, aMethodInfo, null, false, null);
			aAssembler.StackContents.Pop();
		}
	
		public override void DoAssemble() {
			Assemble(Assembler, mMethodInfo, mCurrentILOffset);
		}
	}
}