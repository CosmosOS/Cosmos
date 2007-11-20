using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class Op: IL.Op {
		public Op(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		public static void Ldarg(Assembler.Assembler aAssembler, MethodInformation.Argument aArg) {
			Ldarg(aAssembler, aArg, true);
		}

		public static void Ldarg(Assembler.Assembler aAssembler, MethodInformation.Argument aArg, bool aAddGCCode) {
			foreach (string xAddress in aArg.VirtualAddresses) {
				new Move(CPUx86.Registers.EAX, "[" + xAddress + "]");
				new Push(CPUx86.Registers.EAX);
			}
			aAssembler.StackSizes.Push(aArg.Size);
			if (aAddGCCode && aArg.IsReferenceType) {
				new CPUx86.Push(CPUx86.Registers.EAX);
				Engine.QueueMethodRef(GCImplementationRefs.IncRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			}
		}

		public static void Ldflda(Assembler.Assembler aAssembler, string aRelativeAddress) {
			new Popd(CPUx86.Registers.EAX);
			new CPUx86.Add(CPUx86.Registers.EAX, aRelativeAddress.Trim().Substring(1));
			new Pushd(CPUx86.Registers.EAX);
			aAssembler.StackSizes.Push(4);
		}

		public static void Multiply(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			new CPUx86.Pop("eax");
			new CPUx86.Multiply("dword [esp]");
			new CPUx86.Add("esp", "4");
			new Pushd("eax");
		}

		public static void Ldfld(Assembler.Assembler aAssembler, TypeInformation.Field aField) {
			aAssembler.StackSizes.Pop();
			new CPUx86.Pop("ecx");
			new CPUx86.Add("ecx", "0x" + (aField.Offset).ToString("X"));
			if (aField.Size >= 4) {
				for (int i = 0; i < (aField.Size / 4); i++) {
					new CPUx86.Move("eax", "[ecx + 0x" + (i * 4).ToString("X") + "]");
					new CPUx86.Pushd("eax");
				}
				switch (aField.Size % 4) {
					case 1: {
							new CPUx86.Move("eax", "0");
							new CPUx86.Move("al", "[ecx + 0x" + (aField.Size - 1).ToString("X") + "]");
							new CPUx86.Push("eax");
							break;
						}
					case 2: {
							new CPUx86.Move("eax", "0");
							new CPUx86.Move("ax", "[ecx + 0x" + (aField.Size - 2).ToString("X") + "]");
							new CPUx86.Push("eax");
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
							new CPUx86.Move("eax", "0");
							new CPUx86.Move("al", "[ecx]");
							new CPUx86.Push("eax");
							break;
						}
					case 2: {
							new CPUx86.Move("eax", "0");
							new CPUx86.Move("ax", "[ecx]");
							new CPUx86.Push("eax");
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
			new CPUx86.Move("ecx", "[esp + 0x" + xRoundedSize.ToString("X") + "]");
			new CPUx86.Add("ecx", "0x" + aField.Offset.ToString("X"));
			for (int i = 1; i <= (aField.Size / 4); i++) {
				new CPUx86.Pop("eax");
				new Move("dword [ecx + 0x" + (aField.Size - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (aField.Size % 4) {
				case 1: {
						new CPUx86.Pop("eax");
						new Move("byte [ecx]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop("eax");
						new Move("word [ecx]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aField.Size % 4) + " not supported!");

			}
			new CPUx86.Add("esp", "4");
			aAssembler.StackSizes.Pop();
		}

		public static void Add(Assembler.Assembler aAssembler) {
			aAssembler.StackSizes.Pop();
			new CPUx86.Pop("eax");
			new CPUx86.Add("eax", "[esp]");
			new CPUx86.Add("esp", "4");
			new Pushd("eax");
		}

		public static void Ldloc(Assembler.Assembler aAssembler, MethodInformation.Variable aLocal) {
			Ldloc(aAssembler, aLocal, true);
		}

		public static void Ldloc(Assembler.Assembler aAssembler, MethodInformation.Variable aLocal, bool aAddGCCode) {
			foreach (string s in aLocal.VirtualAddresses) {
				new CPUx86.Move("eax", "[" + s + "]");
				new CPUx86.Push("eax");
			}
			aAssembler.StackSizes.Push(aLocal.Size);
			if (aAddGCCode && aLocal.IsReferenceType) {
				new CPUx86.Push("eax");
				Engine.QueueMethodRef(GCImplementationRefs.IncRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			}
		}
	}
}