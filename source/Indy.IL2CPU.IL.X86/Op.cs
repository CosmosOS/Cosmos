using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

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

		[Obsolete("Try using specialized opcodes")]
		protected void Literal(string aData) {
			Assembler.Add(new Literal(aData));
		}

		protected void Comment(string aText) {
			Assembler.Add(new Comment(aText));
		}

		public static void Push(Assembler.Assembler aAssembler, int aSize, params string[] aArguments) {
			aAssembler.Add(new Push(aArguments));
			aAssembler.StackSizes.Push(aSize);
		}

		protected void Pushd(int aSize, params string[] aArguments) {
			Assembler.Add(new Pushd(aArguments));
			Assembler.StackSizes.Push(aSize);
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

		public static void Ldarg(Assembler.Assembler aAssembler, string[] aAddresses, int aSize) {
			foreach (string xAddress in aAddresses) {
				Move(aAssembler, "eax", "[" + xAddress + "]");
				aAssembler.Add(new Push("eax"));
			}
			aAssembler.StackSizes.Push(aSize);
		}

		public static void Ldflda(Assembler.Assembler aAssembler, string aRelativeAddress) {
			aAssembler.Add(new Popd("eax"));
			aAssembler.Add(new CPU.Add("eax", aRelativeAddress.Trim().Substring(1)));
			aAssembler.Add(new Pushd("eax"));
			aAssembler.StackSizes.Push(4);
		}

		public static void Multiply(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			aAssembler.Add(new CPU.Pop("eax"));
			aAssembler.Add(new CPU.Multiply("dword [esp]"));
			aAssembler.Add(new CPU.Add("esp", "4"));
			aAssembler.Add(new Pushd("eax"));
		}

		public static void Ldfld(Assembler.Assembler aAssembler, TypeInformation.Field aField) {
			aAssembler.StackSizes.Pop();
			aAssembler.Add(new CPU.Pop("ecx"));
			aAssembler.Add(new CPU.Add("ecx", "0x" + (aField.Offset).ToString("X")));
			if (aField.Size >= 4) {
				for (int i = 0; i < (aField.Size / 4); i++) {
					//	Pop("eax");
					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
					aAssembler.Add(new CPU.Move("eax", "[ecx + 0x" + (i * 4).ToString("X") + "]"));
					aAssembler.Add(new CPU.Pushd("eax"));
				}
				switch (aField.Size % 4) {
					case 1: {
							aAssembler.Add(new CPU.Move("eax", "0"));
							aAssembler.Add(new CPU.Move("al", "[ecx + 0x" + (aField.Size - 1).ToString("X") + "]"));
							aAssembler.Add(new CPU.Push("eax"));
							break;
						}
					case 2: {
							aAssembler.Add(new CPU.Move("eax", "0"));
							aAssembler.Add(new CPU.Move("ax", "[ecx + 0x" + (aField.Size - 2).ToString("X") + "]"));
							aAssembler.Add(new CPU.Push("eax"));
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");
				}
			} else {
				switch (aField.Size) {
					case 1: {
							aAssembler.Add(new CPU.Move("eax", "0"));
							aAssembler.Add(new CPU.Move("al", "[ecx]"));
							aAssembler.Add(new CPU.Push("eax"));
							break;
						}
					case 2: {
							aAssembler.Add(new CPU.Move("eax", "0"));
							aAssembler.Add(new CPU.Move("ax", "[ecx]"));
							aAssembler.Add(new CPU.Push("eax"));
							break;
						}
					case 0: {
							break;
						}
					default:
						throw new Exception("Remainder size " + (aField.Size) + " not supported!");
				}
			}
			aAssembler.StackSizes.Push(aField.Size);
		}

		public static void Stfld(Assembler.Assembler aAssembler, TypeInformation.Field aField) {
			aAssembler.StackSizes.Pop();
			int xRoundedSize = aField.Size;
			if(xRoundedSize % 4 != 0) {
				xRoundedSize += 4 - (xRoundedSize % 4);
			}
			aAssembler.Add(new CPU.Move("ecx", "[esp + 0x" + xRoundedSize.ToString("X") + "]"));
			aAssembler.Add(new CPU.Add("ecx", "0x" + aField.Offset.ToString("X")));
			for (int i = 1; i <= (aField.Size / 4); i++) {
				aAssembler.Add(new CPU.Pop("eax"));
				Move(aAssembler, "dword [ecx + 0x" + (aField.Size - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (aField.Size % 4) {
				case 1: {
						aAssembler.Add(new CPU.Pop("eax"));
						Move(aAssembler, "byte [ecx]", "al");
						break;
					}
				case 2: {
						aAssembler.Add(new CPU.Pop("eax"));
						Move(aAssembler, "word [ecx]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");

			}
			aAssembler.Add(new CPU.Add("esp", "4"));
			aAssembler.StackSizes.Pop();
		}

		public void Multiply() {
			Multiply(Assembler);
		}

		public static void Add(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			aAssembler.Add(new CPU.Pop("eax"));
			aAssembler.Add(new CPU.Add("eax", "[esp]"));
			aAssembler.Add(new CPU.Add("esp", "4"));
			aAssembler.Add(new Pushd("eax"));
		}

		public void Add() {
			Add(Assembler);
		}
	}
}