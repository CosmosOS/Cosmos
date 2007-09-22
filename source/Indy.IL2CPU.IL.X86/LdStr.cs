using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler.Assembler;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldstr, false)]
	public class LdStr: Op {
		public readonly string LiteralStr;
		public LdStr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			LiteralStr = (string)aInstruction.Operand;
		}

		public override void DoAssemble() {
			// Make sure the crawler finds string constructors
			string xDataName = Assembler.GetIdentifier("StringLiteral");
			var xDataByteArray = new StringBuilder();
			foreach (byte x in Encoding.UTF32.GetBytes(LiteralStr)) {
				xDataByteArray.Append(x.ToString());
				xDataByteArray.Append(",");
			}
			xDataByteArray.Append("0,");
			Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString().TrimEnd(',')));
			Pushd("0" + LiteralStr.Length + "d");
			Func<Op, bool> AssembleOtherOp = delegate(Op aOp) {
				aOp.Assembler = Assembler;
				aOp.Assemble();
				return true;
			};
			AssembleOtherOp(new Newarr(Engine.GetTypeDefinition("mscorlib", "System.Int32")));
			AssembleOtherOp(new Dup(null, null));
			Pushd(xDataName);
			AssembleOtherOp(new Call(Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.Runtime.CompilerServices.RuntimeHelpers"), "InitializeArray", "System.Array", "System.RuntimeFieldHandle")));
			AssembleOtherOp(new Dup(null, null));
			Pushd("0");
			Pushd("0" + LiteralStr.Length.ToString("X") + "h");
			AssembleOtherOp(new Call(Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]", "System.Int32", "System.Int32")));
		}
	}
}