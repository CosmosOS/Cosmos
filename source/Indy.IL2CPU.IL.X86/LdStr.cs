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
			if (Assembler.InMetalMode) {
				// todo: see if we need to output trailing bytes 00 00 or 00 01 depending on whether there are bytes >7F
				string xDataName = Assembler.GetIdentifier("StringLiteral");
				var xDataByteArray = new StringBuilder();
				xDataByteArray.Append(Encoding.ASCII.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
				foreach (byte x in Encoding.ASCII.GetBytes(LiteralStr)) {
					xDataByteArray.Append(x.ToString());
					xDataByteArray.Append(",");
				}
				xDataByteArray.Append("0,");
				Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString().TrimEnd(',')));
				Move(Assembler, "eax", xDataName);
				Pushd("eax");
			} else {
				string xDataName = Assembler.GetIdentifier("StringLiteral");
				var xDataByteArray = new StringBuilder();
				xDataByteArray.Append("0,0,0,0,");
				xDataByteArray.Append(BitConverter.GetBytes((int)InstanceTypeEnum.Array).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes(LiteralStr.Length).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(Encoding.ASCII.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append("0,");
				Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString().TrimEnd(',')));
				Pushd(xDataName);
				new Newobj() {
					Assembler = Assembler,
					CtorRef = Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"),
				}.Assemble();
			}
		}
	}
}