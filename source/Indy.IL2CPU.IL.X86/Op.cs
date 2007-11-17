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

		public static void Ldarg(Assembler.Assembler aAssembler, string[] aAddresses, int aSize) {
			foreach (string xAddress in aAddresses) {
				new Move("eax", "[" + xAddress + "]");
				new Push("eax");
			}
			aAssembler.StackSizes.Push(aSize);
		}

		public static void Ldflda(Assembler.Assembler aAssembler, string aRelativeAddress) {
			new Popd("eax");
			new CPU.Add("eax", aRelativeAddress.Trim().Substring(1));
			new Pushd("eax");
			aAssembler.StackSizes.Push(4);
		}

		public static void Multiply(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			new CPU.Pop("eax");
			new CPU.Multiply("dword [esp]");
			new CPU.Add("esp", "4");
			new Pushd("eax");
		}

		public static void Ldfld(Assembler.Assembler aAssembler, TypeInformation.Field aField) {
			aAssembler.StackSizes.Pop();
			new CPU.Pop("ecx");
			new CPU.Add("ecx", "0x" + (aField.Offset).ToString("X"));
			if (aField.Size >= 4) {
				for (int i = 0; i < (aField.Size / 4); i++) {
					new CPU.Move("eax", "[ecx + 0x" + (i * 4).ToString("X") + "]");
					new CPU.Pushd("eax");
				}
				switch (aField.Size % 4) {
					case 1: {
							new CPU.Move("eax", "0");
							new CPU.Move("al", "[ecx + 0x" + (aField.Size - 1).ToString("X") + "]");
							new CPU.Push("eax");
							break;
						}
					case 2: {
							new CPU.Move("eax", "0");
							new CPU.Move("ax", "[ecx + 0x" + (aField.Size - 2).ToString("X") + "]");
							new CPU.Push("eax");
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
							new CPU.Move("eax", "0");
							new CPU.Move("al", "[ecx]");
							new CPU.Push("eax");
							break;
						}
					case 2: {
							new CPU.Move("eax", "0");
							new CPU.Move("ax", "[ecx]");
							new CPU.Push("eax");
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
			if (xRoundedSize % 4 != 0) {
				xRoundedSize += 4 - (xRoundedSize % 4);
			}
			new CPU.Move("ecx", "[esp + 0x" + xRoundedSize.ToString("X") + "]");
			new CPU.Add("ecx", "0x" + aField.Offset.ToString("X"));
			for (int i = 1; i <= (aField.Size / 4); i++) {
				new CPU.Pop("eax");
				new Move("dword [ecx + 0x" + (aField.Size - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (aField.Size % 4) {
				case 1: {
						new CPU.Pop("eax");
						new Move("byte [ecx]", "al");
						break;
					}
				case 2: {
						new CPU.Pop("eax");
						new Move("word [ecx]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");

			}
			new CPU.Add("esp", "4");
			aAssembler.StackSizes.Pop();
		}

		public static void Add(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			new CPU.Pop("eax");
			new CPU.Add("eax", "[esp]");
			new CPU.Add("esp", "4");
			new Pushd("eax");
		}
	}
}