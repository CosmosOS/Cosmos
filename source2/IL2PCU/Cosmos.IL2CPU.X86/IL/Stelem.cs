using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stelem )]
    public class Stelem : ILOp
    {
        public Stelem( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Assembler.Stack.Pop();

            Stelem_Ref.Assemble( Assembler, ( uint )xSize.Size, aMethod, aOpCode );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.Linq;
        // using Indy.IL2CPU.Assembler;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Stelem)]
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
