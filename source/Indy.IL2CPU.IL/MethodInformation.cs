using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class MethodInformation {
		public struct Variable {
			public Variable(int aOffset, int aSize, bool aIsReferenceTypeField, TypeReference aVariableType) {
				Offset = aOffset;
				Size = aSize;
				VirtualAddresses = new string[Size / 4];
				for (int i = 0; i < (Size / 4); i++) {
					VirtualAddresses[i] = "ebp - 0" + (Offset + ((i + 1) * 4) + 0).ToString("X") + "h";
				}
				IsReferenceType = aIsReferenceTypeField;
				VariableType = aVariableType;
			}
			public readonly int Offset;
			public readonly int Size;
			public readonly bool IsReferenceType;
			public readonly TypeReference VariableType;
			/// <summary>
			/// Gives the list of addresses to access this variable. This field contains multiple entries if the <see cref="Size"/> is larger than 4.
			/// </summary>
			public readonly string[] VirtualAddresses;
		}

		public struct Argument {
			public enum KindEnum {
				In,
				ByRef,
				Out
			}
			public Argument(int aSize, int aOffset, KindEnum aKind, bool mIsReferenceType, TypeReference aArgumentType) {
				Size = aSize;
				Offset = aOffset;
				VirtualAddresses = new string[Size / 4];
				for (int i = 0; i < (Size / 4); i++) {
					VirtualAddresses[i] = "ebp + 0" + (Offset + ((i + 1) * 4) + 4).ToString("X") + "h";
				}
				Kind = aKind;
				ArgumentType = aArgumentType;
				IsReferenceType = mIsReferenceType;
			}

			public readonly string[] VirtualAddresses;
			public readonly int Size;
			public readonly bool IsReferenceType;
			public readonly int Offset;
			public readonly KindEnum Kind;
			public readonly TypeReference ArgumentType;
		}

		public MethodInformation(string aLabelName, Variable[] aLocals, Argument[] aArguments, int aReturnSize, bool aIsInstanceMethod, TypeInformation aTypeInfo, MethodDefinition aMethodDef) {
			Locals = aLocals;
			LabelName = aLabelName;
			Arguments = aArguments;
			ReturnSize = aReturnSize;
			IsInstanceMethod = aIsInstanceMethod;
			TypeInfo = aTypeInfo;
			MethodDefinition = aMethodDef;
		}

		public readonly MethodDefinition MethodDefinition;
		public readonly string LabelName;
		public readonly Variable[] Locals;
		public readonly Argument[] Arguments;
		public readonly int ReturnSize;
		public readonly bool IsInstanceMethod;
		public readonly TypeInformation TypeInfo;
		public override string ToString() {
			StringBuilder xSB = new StringBuilder();
			xSB.AppendLine(String.Format("Method '{0}'\r\n", LabelName));
			xSB.AppendLine("Locals:");
			if (Locals.Length == 0) {
				xSB.AppendLine("\t(none)");
			}
			int xCurIndex = 0;
			foreach (Variable xVar in Locals) {
				xSB.AppendFormat("\t({0}) {1}\t{2}\t{3}\r\n\r\n", xCurIndex++, xVar.Offset, xVar.Size, xVar.VirtualAddresses.FirstOrDefault());
				for (int i = 1; i < xVar.VirtualAddresses.Length; i++) {
					xSB.AppendFormat("\t\t\t{0}\r\n\r\n", xVar.VirtualAddresses[i]);
				}
			}
			xSB.AppendLine("Arguments:");
			if (Arguments.Length == 0) {
				xSB.AppendLine("\t(none)");
			}
			xCurIndex = 0;
			foreach (Argument xArg in Arguments) {
				xSB.AppendLine(String.Format("\t({0}) {1}\t{2}\t{3}\r\n", xCurIndex++, xArg.Offset, xArg.Size, xArg.VirtualAddresses.FirstOrDefault()));
			}
			xSB.AppendLine("\tReturnSize: " + ReturnSize);
			return xSB.ToString();
		}
	}
}