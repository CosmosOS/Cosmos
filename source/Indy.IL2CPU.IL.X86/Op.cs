using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class Op: Indy.IL2CPU.IL.Op {

		protected void Call(string aAddress) {
			Assembler.Add(new CPU.Call(aAddress));
		}

		protected void Label(string aName) {
			Assembler.Add(new Assembler.Label(aName));
		}

		protected void Compare(string aAddress1, string aAddress2) {
			Assembler.Add(new CPU.Compare(aAddress1, aAddress2));
		}

		protected void Move(string aDestination, string aSource) {
			Assembler.Add(new CPU.Move(aDestination, aSource));
		}

		protected void Test(string aArg1, string aArg2) {
			Assembler.Add(new CPU.Test(aArg1, aArg2));
		}

		protected void JumpIfZero(string aAddress) {
			Assembler.Add(new CPU.JumpIfZero(aAddress));
		}

		protected void JumpIfEquals(string aAddress) {
			Assembler.Add(new CPU.JumpIfEquals(aAddress));
		}

		protected void JumpIfGreater(string aAddress) {
			Assembler.Add(new CPU.JumpIfGreater(aAddress));
		}

		protected void JumpIfNotZero(string aAddress) {
			Assembler.Add(new CPU.JumpIfNotZero(aAddress));
		}

		protected void JumpAlways(string aAddress) {
			Assembler.Add(new CPU.JumpAlways(aAddress));
		}

		protected void Literal(string aData) {
			Assembler.Add(new Assembler.Literal(aData));	
		}

		protected void Push(params string[] aArguments) {
			Assembler.Add(new CPU.Push(aArguments));
		}

		protected void Xor(string aArgument1, string aArgument2) {
			Assembler.Add(new CPU.Xor(aArgument1, aArgument2));
		}

		protected void Pop(params string[] aArguments) {
			Assembler.Add(new CPU.Pop(aArguments));
		}

		protected void Ret() {
			Assembler.Add(new CPU.Ret());
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
