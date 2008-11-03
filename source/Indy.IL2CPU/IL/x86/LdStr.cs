using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86.X;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler.Assembler;
using System.Collections.Generic;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Ldstr)]
    public class LdStr : Op {
        private static Dictionary<string, DataMember> mDataMemberMap = new Dictionary<string, DataMember>();
        public readonly string LiteralStr;
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Engine.RegisterType(typeof(string));
        }

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
            byte[] xByteArray = new byte[16 + xEncoding.GetByteCount(aLiteral)];
            var xTemp = BitConverter.GetBytes(Engine.RegisterType(Engine.GetType("mscorlib",
                                                                                 "System.Array")));
            Array.Copy(xTemp, 0, xByteArray, 0, 4);
            xTemp = BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray);
            Array.Copy(xTemp, 0, xByteArray, 4, 4);
            xTemp = BitConverter.GetBytes(aLiteral.Length);
            Array.Copy(xTemp, 0, xByteArray, 8, 4);
            xTemp = BitConverter.GetBytes(2);
            Array.Copy(xTemp, 0, xByteArray, 12, 4);
            xTemp = xEncoding.GetBytes(aLiteral);
            Array.Copy(xTemp, 0, xByteArray, 16, xTemp.Length);
            DataMember xDataMember = null;
            if (!mDataMemberMap.TryGetValue(aLiteral, out xDataMember)) {
                string xDataName = aAssembler.GetIdentifier("StringLiteral");
                object[] xObjectData = new object[7];
                xObjectData[0] = ((uint)Engine.RegisterType(Engine.GetType("mscorlib", "System.String")));
                xObjectData[1] = ((uint)InstanceTypeEnum.StaticEmbeddedObject);
                xObjectData[2] = 1;
                xObjectData[3] = new ElementReference(xDataName + "__Contents");
                xObjectData[4] = new ElementReference(xDataName + "__Contents", 16);
                xObjectData[5] = aLiteral.Length;
                xObjectData[6] = aLiteral.Length;
                aAssembler.DataMembers.Add(new DataMember(xDataName, xObjectData));
                aAssembler.DataMembers.Add(xDataMember = new DataMember(xDataName + "__Contents", xByteArray));
                mDataMemberMap.Add(aLiteral, xDataMember);
                return xDataName;
            } else {
                return xDataMember.Name.Substring(0, xDataMember.Name.Length - "__Contents".Length);
            }
        }

        public override void DoAssemble() {
            var Y = new Y86();
            string xDataName = GetContentsArrayName(Assembler, LiteralStr);
            new Comment("String Value: " + LiteralStr);
            Y.EAX = Y.Reference(xDataName);
            Y.EAX.Push();
            Assembler.StackContents.Push(new StackContent(4, typeof(string)));
        }
    }
}
