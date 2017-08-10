using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;

using XSharp.Common;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Sizeof )]
    public class Sizeof : ILOp
    {
        public Sizeof( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpType xType = ( OpType )aOpCode;
            uint xSize = SizeOfType( xType.Value );
            XS.Push(xSize);
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.Compiler;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Sizeof)]
        // 	public class Sizeof: Op {
        //         private Type mType;
        //
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    Type xTypeRef = aReader.OperandValueType;
        //         //    if (xTypeRef == null)
        //         //    {
        //         //        throw new Exception("Type not found!");
        //         //    }
        //         //    Engine.RegisterType(xTypeRef);
        //         //}
        //
        // 		public Sizeof(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mType = aReader.OperandValueType;
        // 			if (mType == null) {
        // 				throw new Exception("Type not found!");}
        // 		}
        // 		public override void DoAssemble() {
        //             uint xSize;
        //             GetService<IMetaDataInfoService>().GetTypeFieldInfo(mType, out xSize);
        //             XS.Push(xSize);
        // 			Assembler.Stack.Push(new StackContent(4, typeof(int)));
        // 		}
        // 	}
        // }

    }
}
