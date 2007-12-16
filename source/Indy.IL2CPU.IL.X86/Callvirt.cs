using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Callvirt, true)]
	public class Callvirt: Op {
		private readonly int mMethodIdentifier;
		private readonly bool mHasReturn;
		private readonly string mNormalAddress;
		private readonly string mMethodDescription;
		private readonly int mThisOffset;
		private readonly int mArgumentCount;
		public Callvirt(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xThisOffSet = (from item in aMethodInfo.Locals
							   select item.Offset + item.Size).LastOrDefault();
			MethodReference xMethod = aInstruction.Operand as MethodReference;
			if (xMethod == null) {
				throw new Exception("Unable to determine Method!");
			}
			MethodDefinition xMethodDef = Engine.GetDefinitionFromMethodReference(xMethod);
			mMethodDescription = CPU.Label.GenerateLabelName(xMethodDef);
			if (xMethodDef.IsStatic || !xMethodDef.IsVirtual) {
				Engine.QueueMethod(xMethodDef);
				mNormalAddress = CPU.Label.GenerateLabelName(xMethodDef);
				mHasReturn = !xMethod.ReturnType.ReturnType.FullName.StartsWith("System.Void");
				return;
			}
			mMethodIdentifier = Engine.GetMethodIdentifier(xMethodDef);
			Engine.QueueMethodRef(VTablesImplRefs.GetMethodAddressForTypeRef);
			MethodInformation xTheMethodInfo = Engine.GetMethodInfo(xMethodDef, xMethodDef, mMethodDescription, null);
			mArgumentCount = xTheMethodInfo.Arguments.Length;
			mHasReturn = xTheMethodInfo.ReturnSize != 0;
			mThisOffset = xTheMethodInfo.Arguments[0].Offset;
		}

		public override void DoAssemble() {
			if (!String.IsNullOrEmpty(mNormalAddress)) {
				new CPUx86.Call(mNormalAddress);
			} else {
				if (Assembler.InMetalMode) {
					throw new Exception("Virtual methods not supported in Metal mode! (Called method = '" + mMethodDescription + "')");
				}
				//Assembler.Add(new CPUx86.Pop("eax"));
				//Assembler.Add(new CPUx86.Pushd("eax"));
				new CPUx86.Move(CPUx86.Registers.EAX, "[esp + 0x" + mThisOffset.ToString("X") + "]");
				new CPUx86.Pushd(CPUx86.Registers.AtEAX);
				new CPUx86.Pushd("0" + mMethodIdentifier.ToString("X") + "h");
				new CPUx86.Call(CPU.Label.GenerateLabelName(VTablesImplRefs.GetMethodAddressForTypeRef));
				new CPUx86.Call(CPUx86.Registers.EAX);
			}
			for(int i = 0; i < mArgumentCount;i++) {
				Assembler.StackSizes.Pop();
			}
			if (mHasReturn) {
				new CPUx86.Pushd(CPUx86.Registers.EAX);
				Assembler.StackSizes.Push(4);
			}
		}
	}
}