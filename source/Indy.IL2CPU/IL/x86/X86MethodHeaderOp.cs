//#define EXT_DEBUG
using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodHeaderOp: MethodHeaderOp {
		public readonly MethodInformation.Variable[] Locals;
		public readonly string LabelName = "";
		public readonly MethodInformation.Argument[] Args;
		public readonly bool DebugMode;
	    public readonly bool MethodIsNonDebuggable;
		public X86MethodHeaderOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			LabelName = aMethodInfo.LabelName;
			Args = aMethodInfo.Arguments.ToArray();
			Locals = aMethodInfo.Locals.ToArray();
			DebugMode = aMethodInfo.DebugMode;
		    MethodIsNonDebuggable = aMethodInfo.IsNonDebuggable;
		}

		public override void DoAssemble() {
			// TODO: add support for variables with a diff datasize, other than 32bit
            AssembleHeader(Assembler, LabelName, Locals, Args, DebugMode, MethodIsNonDebuggable);
		}

		public static void AssembleHeader(Assembler.Assembler aAssembler, string aLabelName, MethodInformation.Variable[] aLocals, MethodInformation.Argument[] aArguments, bool aDebugMode, bool aIsNonDebuggable) {
			new CPU.Label(aLabelName);
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EBP };
#if EXT_DEBUG
				new CPUx86.Move("edi", LdStr.GetContentsArrayName(aAssembler, aLabelName) + "__Contents");
				new CPUx86.Add("edi", "0x10");
#endif
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EBP, SourceReg = CPUx86.Registers.ESP };
			//new CPUx86.Push("0");
			//if (!(aLabelName.Contains("Cosmos.Kernel.Serial") || aLabelName.Contains("Cosmos.Kernel.Heap"))) {
			//    new CPUx86.Push(LdStr.GetContentsArrayName(aAssembler, aLabelName));
			//    MethodBase xTempMethod = Engine.GetMethodBase(Engine.GetType("Cosmos.Kernel", "Cosmos.Kernel.Serial"), "Write", "System.Byte", "System.String");
			//    new CPUx86.Call(Label.GenerateLabelName(xTempMethod));
			//    Engine.QueueMethod(xTempMethod);
			//}
			foreach (var xLocal in aLocals) {
				aAssembler.StackContents.Push(new StackContent(xLocal.Size, xLocal.VariableType));
				for (int i = 0; i < (xLocal.Size / 4); i++) {
					new CPUx86.Push{DestinationValue=0};
				}
			}
			if(aDebugMode&& aIsNonDebuggable) {
                new CPUx86.Call { DestinationLabel = "DebugPoint_DebugSuspend" };
			}
		}
	}
}