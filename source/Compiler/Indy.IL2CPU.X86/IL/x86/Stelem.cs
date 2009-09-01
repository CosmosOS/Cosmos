using System;
using System.Collections.Generic;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Compiler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stelem)]
	public class Stelem: Op {
		private Type mType;
		private string mNextLabel;
	    private string mCurLabel;
	    private uint mCurOffset;
	    private MethodInformation mMethodInformation;
        
        public Stelem(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mType = aReader.OperandValueType;
			if (mType == null)
				throw new Exception("Unable to determine Type!");
             mMethodInformation = aMethodInfo;
		    mCurOffset = aReader.Position;
		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
            mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		}

		public override void DoAssemble() {
            var xElementSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
		    new Comment("Element size: " + xElementSize);
            Stelem_Ref.Assemble(Assembler, xElementSize, GetServiceProvider(), mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		}
	}
}