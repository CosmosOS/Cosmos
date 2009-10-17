using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldftn )]
    public class Ldftn : ILOp
    {
        public Ldftn( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Push { DestinationRef = ElementReference.New( MethodInfoLabelGenerator.GenerateLabelName(((OpMethod)aOpCode).Value ) ) };
            Assembler.Stack.Push( new StackContents.Item( 4, true, false, false ) );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.Linq;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldftn)]
        // 	public class Ldftn: Op {
        // 		private string mFunctionLabel;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    MethodBase xMethodRef = aReader.OperandValueMethod;
        //         //    if (xMethodRef == null)
        //         //    {
        //         //        throw new Exception("Unable to determine Method!");
        //         //    }
        //         //    Engine.QueueMethod(xMethodRef);
        //         //}
        // 
        // 		public Ldftn(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			MethodBase xMethodRef = aReader.OperandValueMethod;
        // 			if (xMethodRef == null) {
        // 				throw new Exception("Unable to determine Method!");
        // 			}
        // 			mFunctionLabel = MethodInfoLabelGenerator.GenerateLabelName(xMethodRef);
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             new CPUx86.Push { DestinationRef = ElementReference.New(mFunctionLabel) };
        // 			Assembler.Stack.Push(new StackContent(4, true, false, false));
        // 		}
        // 	}
        // }

    }
}
