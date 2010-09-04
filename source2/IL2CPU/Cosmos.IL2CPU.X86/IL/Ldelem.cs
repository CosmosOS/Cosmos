using System;
using Cosmos.IL2CPU.ILOpCodes;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem)]
  public class Ldelem: ILOp {
    public Ldelem(Cosmos.Compiler.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpType = (OpType)aOpCode;
      var xSize = SizeOfType(xOpType.Value);
      Ldelem_Ref.Assemble(Assembler, (uint)xSize);
    }


    // using System;
    // using System.Collections.Generic;
    // using System.Linq;
    // using Cosmos.IL2CPU.X86;
    // using Cosmos.IL2CPU.Compiler;
    // 
    // namespace Cosmos.IL2CPU.IL.X86 {
    // 	[OpCode(OpCodeEnum.Ldelem)]
    // 	public class Ldelem: Op {
    //         private Type mType;
    //         
    //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
    //         //{
    //         //    Type xType = aReader.OperandValueType;
    //         //    if (xType == null)
    //         //        throw new Exception("Unable to determine Type!");
    //         //    Engine.RegisterType(xType);
    //         //}
    // 
    //         public Ldelem(ILReader aReader, MethodInformation aMethodInfo)
    // 			: base(aReader, aMethodInfo) {
    // 			mType = aReader.OperandValueType;
    // 			if (mType == null)
    // 				throw new Exception("Unable to determine Type!");
    // 			
    // 		}
    // 
    // 		public override void DoAssemble() {
    //             var xElementSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
    //             new Comment("Element size: " + xElementSize);
    // 			Ldelem_Ref.Assemble(Assembler, xElementSize);
    // 		}
    // 	}
    // }

  }
}
