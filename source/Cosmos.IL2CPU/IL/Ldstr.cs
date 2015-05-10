using System;
using System.Linq;
using CPU = Cosmos.Assembler;
using System.Text;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
using Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldstr)]
	public class LdStr: ILOp
	{
		public LdStr(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpString = aOpCode as OpString;
      string xDataName = GetContentsArrayName(xOpString.Value);
      new Comment( Assembler, "String Value: " + xOpString.Value.Replace( "\r", "\\r" ).Replace( "\n", "\\n" ) );
      new Mov { DestinationReg = RegistersEnum.EAX, SourceRef = Cosmos.Assembler.ElementReference.New(xDataName) };
      new Push { DestinationReg = RegistersEnum.EAX };
      // DEBUG VERIFICATION: leave it here for now. we have issues with fields ordering. if that changes, we need to change the code below!
      #region Debug verification

      var xFields = GetFieldsInfo(typeof(string)).Where(i => !i.IsStatic).ToArray();
      if (xFields[0].Id != "System.Int32 System.String.m_stringLength"
        || xFields[0].Offset != 0) {
        throw new Exception("Fields changed!");
      }
      if (xFields[1].Id != "System.Char System.String.m_firstChar"
        || xFields[1].Offset != 4) {
        throw new Exception("Fields changed!");
      }
      #endregion
    }

    public static string GetContentsArrayName(string aLiteral) {
      var xAsm = CPU.Assembler.CurrentInstance;

      Encoding xEncoding = Encoding.Unicode;

      string xDataName = xAsm.GetIdentifier("StringLiteral");
        var xBytecount = xEncoding.GetByteCount(aLiteral);
      var xObjectData = new byte[(4*4) + (xBytecount)];
      Array.Copy(BitConverter.GetBytes((int)-1), 0, xObjectData, 0, 4);
      Array.Copy(BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedObject), 0, xObjectData, 4, 4);
      Array.Copy(BitConverter.GetBytes((int)1), 0, xObjectData, 8, 4);
      Array.Copy(BitConverter.GetBytes(aLiteral.Length), 0, xObjectData, 12, 4);
      Array.Copy(xEncoding.GetBytes(aLiteral), 0, xObjectData, 16, xBytecount);
      xAsm.DataMembers.Add(new CPU.DataMember(xDataName, xObjectData));
      return xDataName;
    }

		// using System;
		// using System.Linq;
		// using System.Text;
		// using Cosmos.IL2CPU.X86;
		// using Cosmos.IL2CPU.X86.X;
		// using CPUx86 = Cosmos.Assembler.x86;
		// using Asm = Assembler;
		// using System.Collections.Generic;
		//
		// namespace Cosmos.IL2CPU.IL.X86 {
		//     [Cosmos.Assembler.OpCode(OpCodeEnum.Ldstr)]
		//     public class LdStr : Op {
		//         //private static Dictionary<string, DataMember> mDataMemberMap = new Dictionary<string, DataMember>();
		//         public readonly string LiteralStr;
		//
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    Engine.RegisterType(typeof(string));
		//         //}
		//
		//         public LdStr(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo) {
		//             LiteralStr = aReader.OperandValueStr;
		//         }
		//
		//         public LdStr(string aLiteralStr)
		//             : base(null, null) {
		//             LiteralStr = aLiteralStr;
		//         }
		//

		//     }
		// }

	}
}
