using System;

using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stelem )]
    public class Stelem : ILOp
    {
        public Stelem( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
          var xOpType = (OpType)aOpCode;
          var xSize = SizeOfType(xOpType.Value);

          Stelem_Ref.Assemble(Assembler, (uint)xSize, aMethod, aOpCode, DebugEnabled);
        }


        // using System;
        // using System.Collections.Generic;
        // using System.Linq;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.Compiler;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Stelem)]
        // 	public class Stelem: Op {
        // 		private Type mType;
        // 		private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        //
        //         public Stelem(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mType = aReader.OperandValueType;
        // 			if (mType == null)
        // 				throw new Exception("Unable to determine Type!");
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        //
        // 		public override void DoAssemble() {
        //             var xElementSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
        // 		    new Comment("Element size: " + xElementSize);
        //             Stelem_Ref.Assemble(Assembler, xElementSize, GetServiceProvider(), mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        // 		}
        // 	}
        // }

    }
}
