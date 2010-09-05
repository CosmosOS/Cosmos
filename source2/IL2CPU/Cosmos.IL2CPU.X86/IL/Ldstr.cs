using System;
using CPU = Cosmos.Compiler.Assembler;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldstr)]
	public class LdStr: ILOp
	{
		public LdStr(Cosmos.Compiler.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpString = aOpCode as OpString;
      string xDataName = GetContentsArrayName(xOpString.Value);
      new Comment( Assembler, "String Value: " + xOpString.Value.Replace( "\r", "\\r" ).Replace( "\n", "\\n" ) );
      new Move { DestinationReg = RegistersEnum.EAX, SourceRef = ElementReference.New(xDataName) };
      new Push { DestinationReg = RegistersEnum.EAX };
      Assembler.Stack.Push(4, typeof(string));
      // DEBUG VERIFICATION: leave it here for now. we have issues with fields ordering. if that changes, we need to change the code below!
      #region Debug verification
      var xFields = GetFieldsInfo(typeof(string));
      if (xFields[0].Id != "$$Storage$$"
        || xFields[0].Offset != 0) {
        throw new Exception("Fields changed!");
      }
      if (xFields[1].Id != "System.Int32 System.String.m_stringLength"
        || xFields[1].Offset != 4) {
        throw new Exception("Fields changed!");
      }
      if (xFields[2].Id != "System.Char System.String.m_firstChar"
        || xFields[2].Offset != 8) {
        throw new Exception("Fields changed!");
      }
      #endregion
    }

    public static string GetContentsArrayName(string aLiteral) {
      var xAsm = CPU.Assembler.CurrentInstance;

      Encoding xEncoding = Encoding.Unicode;
      byte[] xByteArray = new byte[16 + xEncoding.GetByteCount(aLiteral)];
      //throw new NotImplementedException("Todo: implement literal strings");
      //var xTemp = BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib",
      //                                                                     "System.Array")));
      //Array.Copy(xTemp, 0, xByteArray, 0, 4);
      var xTemp = BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray);
      Array.Copy(xTemp, 0, xByteArray, 4, 4);
      xTemp = BitConverter.GetBytes(aLiteral.Length);
      Array.Copy(xTemp, 0, xByteArray, 8, 4);
      xTemp = BitConverter.GetBytes(2);
      Array.Copy(xTemp, 0, xByteArray, 12, 4);
      xTemp = xEncoding.GetBytes(aLiteral);
      Array.Copy(xTemp, 0, xByteArray, 16, xTemp.Length);
      DataMember xDataMember;// = null;
      //if (!mDataMemberMap.TryGetValue(aLiteral, out xDataMember))
      //{
      string xDataName = xAsm.GetIdentifier("StringLiteral");
      object[] xObjectData = new object[7];
      xObjectData[0] = -1;//ElementReference.New(ILOp.GetTypeIDLabel(typeof(String)), 0);
      xObjectData[1] = ((uint)InstanceTypeEnum.StaticEmbeddedObject);
      xObjectData[2] = 1;
      xObjectData[3] = ElementReference.New(xDataName + "__Contents");
      xObjectData[4] = aLiteral.Length;
      xObjectData[5] = ElementReference.New(xDataName + "__Contents", 16);
      xObjectData[6] = aLiteral.Length;
      xAsm.DataMembers.Add(new CPU.DataMember(xDataName, xObjectData));
      xAsm.DataMembers.Add(new CPU.DataMember(xDataName + "__Contents", xByteArray));
      //mDataMemberMap.Add(aLiteral, xDataMember);
      return xDataName;
    }
    
		// using System;
		// using System.Linq;
		// using System.Text;
		// using Cosmos.IL2CPU.X86;
		// using Cosmos.IL2CPU.X86.X;
		// using CPUx86 = Cosmos.Compiler.Assembler.X86;
		// using Asm = Assembler;
		// using System.Collections.Generic;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		//     [OpCode(OpCodeEnum.Ldstr)]
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
