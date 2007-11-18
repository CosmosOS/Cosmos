using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
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
			xDataByteArray.Append(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray).Aggregate("", (r, b) => r + b + ","));
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
				xRefByteArray.Append(",0x" + ((uint)InstanceTypeEnum.StaticEmbeddedObject).ToString("X") + ",0,");
				xRefByteArray.Append(xDataName + "__Contents,");
				xRefByteArray.Append("0,0,0");
				Assembler.DataMembers.Add(new DataMember(xDataName, "dd", xRefByteArray.ToString()));
				Assembler.DataMembers.Add(new DataMember(xDataName + "__Contents", "db", xDataVal));
			} else {
				xDataName = xDataName.Substring(0, xDataName.Length - "__Contents".Length);
			}
			new Comment("String Value: " + LiteralStr);
			new CPUx86.Move(CPUx86.Registers.EAX, xDataName);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			Assembler.StackSizes.Push(4);
		}
	}
}