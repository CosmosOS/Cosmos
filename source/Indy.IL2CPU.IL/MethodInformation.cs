using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class MethodInformation {
		public struct Variable {
			public Variable(int aOffset, int aSize, bool aIsReferenceTypeField) {
				Offset = aOffset;
				Size = aSize;
				VirtualAddress = "ebp - 0" + (Offset + Size + 0).ToString("X") + "h";
				IsReferenceTypeField = aIsReferenceTypeField;
			}
			public readonly int Offset;
			public readonly int Size;
			public readonly string VirtualAddress;
			public readonly bool IsReferenceTypeField;
		}

		public struct Argument {
			public enum KindEnum {
				In,
				ByRef,
				Out
			}
			public Argument(int aSize, int aOffset, KindEnum aKind) {
				Size = aSize;
				Offset = aOffset;
				VirtualAddress = "ebp + 0" + (Offset + Size + 4).ToString("X") + "h";
				Kind = aKind;
			}

			public readonly string VirtualAddress;
			public readonly int Size;
			public readonly int Offset;
			public readonly KindEnum Kind;
		}

		public MethodInformation(string aLabelName, Variable[] aLocals, Argument[] aArguments, bool aHasReturnValue, bool aIsInstanceMethod, TypeInformation aTypeInfo, MethodDefinition aMethodDef) {
			Locals = aLocals;
			LabelName = aLabelName;
			Arguments = aArguments;
			HasReturnValue = aHasReturnValue;
			IsInstanceMethod = aIsInstanceMethod;
			TypeInfo = aTypeInfo;
			MethodDefinition = aMethodDef;
		}

		public readonly MethodDefinition MethodDefinition;
		public readonly string LabelName;
		public readonly Variable[] Locals;
		public readonly Argument[] Arguments;
		public readonly bool HasReturnValue;
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
			foreach(Variable xVar in Locals) {
				xSB.AppendLine(String.Format("\t({0}) {1}\t{2}\t{3}\r\n", xCurIndex++, xVar.Offset, xVar.Size, xVar.VirtualAddress));
			}
			xSB.AppendLine("Arguments:");
			if (Arguments.Length == 0) {
				xSB.AppendLine("\t(none)");
			}
			xCurIndex = 0;
			foreach (Argument xArg in Arguments) {
				xSB.AppendLine(String.Format("\t({0}) {1}\t{2}\t{3}\r\n", xCurIndex++, xArg.Offset, xArg.Size, xArg.VirtualAddress));
			}
			return xSB.ToString();
		}
	}
}