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

		public LdStr(string aLiteralStr):base(null, null) {
			LiteralStr = aLiteralStr;
		}

		public override void DoAssemble() {
			if (Assembler.InMetalMode) {
				var xDataByteArray = new StringBuilder();
				xDataByteArray.Append(BitConverter.GetBytes(LiteralStr.Length).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(Encoding.ASCII.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
				string xDataVal = xDataByteArray.ToString().TrimEnd(',');
				string xDataName = (from item in Assembler.DataMembers
									where item.DefaultValue == xDataVal
									select item.Name).FirstOrDefault();
				if (String.IsNullOrEmpty(xDataName)) {
					xDataName = Assembler.GetIdentifier("StringLiteral");
					Assembler.DataMembers.Add(new DataMember(xDataName, "dd", xDataName + "__Contents"));
					Assembler.DataMembers.Add(new DataMember(xDataName + "__Contents", "db", xDataVal));
				}
				Move(Assembler, "eax", xDataName);
				Pushd(4, "eax");
			} else {
				var xDataByteArray = new StringBuilder();
				// todo: see if we need to output trailing bytes 00 00 or 00 01 depending on whether there are bytes >7F
				xDataByteArray.Append(BitConverter.GetBytes(Engine.RegisterType(Engine.GetTypeDefinition("mscorlib", "System.Array"))).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes((int)InstanceTypeEnum.Array).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(BitConverter.GetBytes(LiteralStr.Length).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append(Encoding.Unicode.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
				xDataByteArray.Append("0,");
				string xDataVal = xDataByteArray.ToString().TrimEnd(',');
				string xDataName = (from item in Assembler.DataMembers
									where item.DefaultValue == xDataVal
									select item.Name).FirstOrDefault();
				if (String.IsNullOrEmpty(xDataName)) {
					xDataName = Assembler.GetIdentifier("StringLiteral");
					//Assembler.DataMembers.Add(new DataMember(xDataName, "dd", xDataName + "__Contents"));
					Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataVal));
				}
				Pushd(4, xDataName);
				new Newobj() {
					Assembler = Assembler,
					CtorDef = Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"),
				}.Assemble();
			}
		}
	}
}