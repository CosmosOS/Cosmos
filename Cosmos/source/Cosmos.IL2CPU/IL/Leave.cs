using System;
using System.Reflection;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Leave )]
    public class Leave : ILOp
    {
        public Leave( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            // apparently, Roslyn changed something to the output. We now have to figure out where to jump to.
            if (aOpCode.CurrentExceptionHandler.Flags.HasFlag(ExceptionHandlingClauseOptions.Finally)
              && aOpCode.CurrentExceptionHandler.HandlerOffset > aOpCode.Position)
            {
                new CPUx86.Jump {DestinationLabel = AppAssembler.TmpPosLabel(aMethod, aOpCode.CurrentExceptionHandler.HandlerOffset)};
            }
            else
            {
                new CPUx86.Jump {DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode)};
            }
        }


        // using System;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Leave)]
        // 	public class Leave: Op {public readonly string TargetLabel;
        // 	public Leave(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
        // 		}
        // 		public override void DoAssemble() {
        //         new CPU.Jump { DestinationLabel = TargetLabel };
        // 		}
        // 	}
        // }

    }
}
