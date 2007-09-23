using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldstr, true)]
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
			xDataByteArray.Append("0,0,0,0,");
			xDataByteArray.Append(BitConverter.GetBytes((int)InstanceTypeEnum.Array).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes(LiteralStr.Length).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(Encoding.ASCII.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append("0,");
			Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString().TrimEnd(',')));
			//Pushd("0" + LiteralStr.Length.ToString("X") + "d");
			Func<Op, bool> AssembleOtherOp = delegate(Op aOp) {
				aOp.Assembler = Assembler;
				aOp.Assemble();
				return true;
			};
			Pushd(xDataName);
			new Newobj() {
				Assembler = Assembler,
				CtorRef = Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"),
			}.Assemble();

		}
	}
}