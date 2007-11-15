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

		public LdStr(string aLiteralStr)
			: base(null, null) {
			LiteralStr = aLiteralStr;
		}

		public override void DoAssemble() {
			Encoding xEncoding;
			if (Assembler.InMetalMode) {
				xEncoding = Encoding.ASCII;
			} else {
				xEncoding = Encoding.Unicode;
			}
			var xDataByteArray = new StringBuilder();
			xDataByteArray.Append(BitConverter.GetBytes(Engine.RegisterType(Engine.GetTypeDefinition("mscorlib", "System.Array"))).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes((int)InstanceTypeEnum.Array).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes(LiteralStr.Length).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(xEncoding.GetBytes(LiteralStr).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append("0,");
			string xDataVal = xDataByteArray.ToString().TrimEnd(',');
			string xDataName = (from item in Assembler.DataMembers
								where item.DefaultValue == xDataVal
								select item.Name).FirstOrDefault();
			if (String.IsNullOrEmpty(xDataName)) {
				xDataName = Assembler.GetIdentifier("StringLiteral");
				StringBuilder xRefByteArray = new StringBuilder();
				xRefByteArray.Append("0x" + ((uint)Engine.RegisterType(Engine.GetTypeDefinition("mscorlib", "System.String"))).ToString("X"));
				xRefByteArray.Append(",0x" + ((int)InstanceTypeEnum.NormalObject).ToString("X") + ",0,");
				xRefByteArray.Append(xDataName + "__Contents,");
				xRefByteArray.Append("0,0,0");
				Assembler.DataMembers.Add(new DataMember(xDataName, "dd", xRefByteArray.ToString()));
				Assembler.DataMembers.Add(new DataMember(xDataName + "__Contents", "db", xDataVal));
			} else {
				xDataName = xDataName.Substring(0, xDataName.Length - "__Contents".Length);
			}
			Comment("String Value: " + LiteralStr);
			Move(Assembler, "eax", xDataName);
			Pushd(4, "eax");
		}
	}
}