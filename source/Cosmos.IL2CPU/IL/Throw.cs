using System;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Throw )]
    public class Throw : ILOp
    {
        public Throw( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
#warning TODO: Implement exception
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationRef = Cosmos.Assembler.ElementReference.New( DataMember.GetStaticFieldName( ExceptionHelperRefs.CurrentExceptionRef ) ), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
            //new CPUx86.Call { DestinationLabel = aExceptionOccurredLabel };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceValue = 3 };
            Call.EmitExceptionLogic( Assembler,aMethod, aOpCode, false, null );
          
        }

        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Throw)]
        // 	public class Throw: Op {
        // 		private MethodInformation mMethodInfo;
        // 		private int mCurrentILOffset;
        //         public Throw(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mMethodInfo = aMethodInfo;
        // 			mCurrentILOffset = (int)aReader.Position;
        // 		}
        // 
        // 		public static void Assemble(Assembler.Assembler aAssembler, MethodInformation aMethodInfo, int aCurrentILOffset, string aExceptionOccurredLabel) {
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Move { DestinationRef = CPU.ElementReference.New(CPU.DataMember.GetStaticFieldName(CPU.Assembler.CurrentExceptionRef)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
        //             new CPUx86.Call { DestinationLabel = aExceptionOccurredLabel };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 3 };
        // 			Call.EmitExceptionLogic(aAssembler, (uint)aCurrentILOffset, aMethodInfo, null, false, null);
        // 			aAssembler.Stack.Pop();
        // 		}
        // 	
        // 		public override void DoAssemble() {
        // 		    var xMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(CPU.Assembler.CurrentExceptionOccurredRef,
        // 		                                                                       false);
        // 			Assemble(Assembler, mMethodInfo, mCurrentILOffset, xMethodInfo.LabelName);
        // 		}
        // 	}
        // }
    }
}
