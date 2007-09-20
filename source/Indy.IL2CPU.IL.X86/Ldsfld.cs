using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private bool IsIntPtrZero = false;
		private string mDataName;
		
		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			IsIntPtrZero = aInstruction.Operand.ToString() == "System.IntPtr System.IntPtr::Zero";
			if (!IsIntPtrZero) {
				DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.FullName, xField.DeclaringType.FullName, xField.Name, out mDataName);
			}
		}
		public override void DoAssemble() {
			if(IsIntPtrZero) {
				Pushd("0");
				return;
			}
			Pushd("[" + mDataName + "]");
		}
	}
}