using System;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class MethodInformation {
		public struct Variable {
			public Variable(int aOffset, int aSize, bool aIsReferenceTypeField, Type aVariableType) {
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
			public readonly Type VariableType;
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
			public Argument(int aSize, int aOffset, KindEnum aKind, bool mIsReferenceType, Type aArgumentType) {
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
			public readonly Type ArgumentType;
		}

		public MethodInformation(string aLabelName, Variable[] aLocals, Argument[] aArguments, int aReturnSize, bool aIsInstanceMethod, TypeInformation aTypeInfo, MethodBase aMethod, Type aReturnType, bool debugMode) {
			Locals = aLocals;
			DebugMode = debugMode;
			LabelName = aLabelName;
			Arguments = aArguments;
			ReturnSize = aReturnSize;
			IsInstanceMethod = aIsInstanceMethod;
			TypeInfo = aTypeInfo;
			Method = aMethod;
			ReturnType = aReturnType;
			LocalsSize = (from item in aLocals
						  let xSize = (item.Size % 4 == 0) ? item.Size : (item.Size + (4 - (item.Size % 4)))
						  select xSize).Sum();
		}

		/// <summary>
		/// This variable is only updated when the MethodInformation instance is supplied by the Engine.ProcessAllMethods method
		/// </summary>
		public ExceptionHandlingClause CurrentHandler;
		public readonly MethodBase Method;
		public readonly string LabelName;
		public readonly Variable[] Locals;
		public readonly Argument[] Arguments;
		public readonly int ReturnSize;
		public readonly Type ReturnType;
		public readonly bool IsInstanceMethod;
		public readonly TypeInformation TypeInfo;
		public readonly int LocalsSize;
		public readonly bool DebugMode;
		public override string ToString() {
			var xSB = new StringBuilder();
			xSB.AppendLine(String.Format("Method '{0}'\r\n", Method.GetFullName()));
			xSB.AppendLine("Locals:");
			if (Locals.Length == 0) {
				xSB.AppendLine("\t(none)");
			}
			var xCurIndex = 0;
			foreach (var xVar in Locals) {
				xSB.AppendFormat("\t({0}) {1}\t{2}\t{3} (Type = {4})\r\n\r\n", xCurIndex++, xVar.Offset, xVar.Size, xVar.VirtualAddresses.FirstOrDefault(), xVar.VariableType.FullName);
			}
			xSB.AppendLine("Arguments:");
			if (Arguments.Length == 0) {
				xSB.AppendLine("\t(none)");
			}
			xCurIndex = 0;
			foreach (var xArg in Arguments) {
				xSB.AppendLine(String.Format("\t({0}) {1}\t{2}\t{3} (Type = {4})\r\n", xCurIndex++, xArg.Offset, xArg.Size, xArg.VirtualAddresses.FirstOrDefault(), xArg.ArgumentType.FullName));
			}
			xSB.AppendLine("\tReturnSize: " + ReturnSize);
			return xSB.ToString();
		}
	}
}