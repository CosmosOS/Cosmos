using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler.Assembler;
using System.Collections.Generic;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldstr, true)]
	public class LdStr: Op {
		private static Dictionary<string, DataMember> mDataMemberMap = new Dictionary<string, DataMember>();
		public readonly string LiteralStr;
		public LdStr(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			LiteralStr = aReader.OperandValueStr;
		}

		public LdStr(string aLiteralStr)
			: base(null, null) {
			LiteralStr = aLiteralStr;
		}

		public static string GetContentsArrayName(Assembler.Assembler aAssembler, string aLiteral) {
			Encoding xEncoding = Encoding.Unicode;
			var xDataByteArray = new StringBuilder();
			xDataByteArray.Append(BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib", "System.Array"))).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes(aLiteral.Length).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(BitConverter.GetBytes((int)2).Aggregate("", (r, b) => r + b + ","));
			xDataByteArray.Append(xEncoding.GetBytes(aLiteral).Aggregate("", (r, b) => r + b + ","));
			string xDataVal = xDataByteArray.ToString().TrimEnd(',');
			DataMember xDataMember = null;
			if (!mDataMemberMap.TryGetValue(xDataVal, out xDataMember)) {
				string xDataName = aAssembler.GetIdentifier("StringLiteral");
				StringBuilder xRefByteArray = new StringBuilder();
				xRefByteArray.Append("0x" + ((uint)Engine.RegisterType(Engine.GetType("mscorlib", "System.String"))).ToString("X"));
				xRefByteArray.Append(",0x" + ((uint)InstanceTypeEnum.StaticEmbeddedObject).ToString("X") + ",");
				xRefByteArray.Append("0x" + (1).ToString("X") + ",");
				xRefByteArray.Append(xDataName + "__Contents,");
				xRefByteArray.Append("0,0,0");
				aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(aAssembler.CurrentGroup, new DataMember(xDataName, "dd", xRefByteArray.ToString())));
				aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(aAssembler.CurrentGroup, xDataMember=new DataMember(xDataName + "__Contents", "db", xDataVal)));
				mDataMemberMap.Add(xDataVal, xDataMember);
				return xDataName;
			} else {
				return xDataMember.Name.Substring(0, xDataMember.Name.Length - "__Contents".Length);
			}
		}

		public override void DoAssemble() {
			string xDataName = GetContentsArrayName(Assembler, LiteralStr);
			new Comment("String Value: " + LiteralStr);
			new CPUx86.Move(CPUx86.Registers.EAX, xDataName);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			Assembler.StackContents.Push(new StackContent(4, typeof(string)));
		}
	}
}