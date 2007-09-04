using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class Op: IL.Op {
		public Op(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		protected void Call(string aAddress) {
			Assembler.Add(new Assembler.X86.Call(aAddress));
		}

		protected void Label(string aName) {
			Assembler.Add(new Label(aName));
		}

		protected void Compare(string aAddress1, string aAddress2) {
			Assembler.Add(new Compare(aAddress1, aAddress2));
		}

		protected void Move(string aDestination, string aSource) {
			Assembler.Add(new Move(aDestination, aSource));
		}

		protected void Test(string aArg1, string aArg2) {
			Assembler.Add(new Test(aArg1, aArg2));
		}

		protected void JumpIfZero(string aAddress) {
			Assembler.Add(new JumpIfZero(aAddress));
		}

		protected void JumpIfEquals(string aAddress) {
			Assembler.Add(new JumpIfEquals(aAddress));
		}

		protected void JumpIfGreater(string aAddress) {
			Assembler.Add(new JumpIfGreater(aAddress));
		}

		protected void JumpIfNotZero(string aAddress) {
			Assembler.Add(new JumpIfNotZero(aAddress));
		}

		protected void JumpAlways(string aAddress) {
			Assembler.Add(new JumpAlways(aAddress));
		}

		protected void Literal(string aData) {
			Assembler.Add(new Literal(aData));
		}

		protected void Push(params string[] aArguments) {
			Assembler.Add(new Push(aArguments));
		}

		protected void Pushd(params string[] aArguments) {
			Assembler.Add(new Pushd(aArguments));
		}

		protected void Xor(string aArgument1, string aArgument2) {
			Assembler.Add(new Assembler.X86.Xor(aArgument1, aArgument2));
		}

		protected void Pop(params string[] aArguments) {
			Assembler.Add(new Assembler.X86.Pop(aArguments));
		}

		protected void Ret() {
			Assembler.Add(new Assembler.X86.Ret(""));
		}

		protected void Invoke(string aProcedureName, params object[] aParams) {
			string xResult = "invoke " + aProcedureName;
			foreach (object o in aParams) {
				xResult += ",";
				if (o != null) {
					xResult += o;
				}
			}
			Literal(xResult);
		}
	}
}