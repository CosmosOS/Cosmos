using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler.X86;
using Instruction=Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class Op: IL.Op {
		public Op(Instruction aInstruction, MethodInformation aMethodInfo)
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

		public static void Move(Assembler.Assembler aAssembler, string aDestination, string aSource) {
			aAssembler.Add(new Move(aDestination, aSource));
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
			Assembler.Add(new JumpIfNotEquals(aAddress));
		}

		protected void JumpAlways(string aAddress) {
			Assembler.Add(new JumpAlways(aAddress));
		}

		protected void Literal(string aData) {
			Assembler.Add(new Literal(aData));
		}

		public static void Push(Assembler.Assembler aAssembler, params string[] aArguments) {
			aAssembler.Add(new Push(aArguments));
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

		public static void Ldarg(Assembler.Assembler aAssembler, string aAddress) {
			Move(aAssembler, "eax", "[" + aAddress + "]");
			Push(aAssembler, "eax");
		}

		public static void Ldflda(Assembler.Assembler aAssembler, string aRelativeAddress) {
			aAssembler.Add(new Popd("eax"));
			aAssembler.Add(new CPU.Add("eax", aRelativeAddress.Trim().Substring(1)));
			aAssembler.Add(new Pushd("eax"));
		}

		public void Multiply() {
			Assembler.Add(new CPU.Pop("eax"));
			Assembler.Add(new CPU.Multiply("dword [esp]"));
			Assembler.Add(new CPU.Add("esp", "4"));
			Pushd("eax");
		}

		public void Add() {
			Assembler.Add(new CPU.Pop("eax"));
			Assembler.Add(new CPU.Add("eax", "[esp]"));
			Assembler.Add(new CPU.Add("esp", "4"));
			Pushd("eax");
		}
	}
}