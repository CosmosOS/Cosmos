using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Switch )]
    public class Switch : ILOp
    {
        public Switch( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            ILOpCodes.OpSwitch OpSw = ( ILOpCodes.OpSwitch )aOpCode; 
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            for( int i = 0; i < OpSw.BranchLocations.Length; i++ )
            {
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = ( uint )i };
                //string DestLabel = AssemblerNasm.TmpBranchLabel( aMethod, new ILOpCodes.OpBranch( ILOpCode.Code.Jmp, aOpCode.Position, OpSw.BranchLocations[ i ] ) );
                string xDestLabel = AppAssembler.TmpPosLabel(aMethod, OpSw.BranchLocations[i]);
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal
                  , DestinationLabel = xDestLabel
                };
            }
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Switch)]
        // 	public class Switch: Op {
        // 		private string[] mLabels;
        // 		public Switch(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			uint[] xCases = aReader.OperandValueBranchLocations;
        // 			mLabels = new string[xCases.Length];
        // 			for (int i = 0; i < xCases.Length; i++) {
        // 			    mLabels[i] = GetInstructionLabel(xCases[i]);
        // 			}
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        // 			for(int i = 0; i < mLabels.Length; i++){
        //                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue =(uint)i };
        //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mLabels[i] };
        // 			}
        // 			Assembler.Stack.Pop();
        // 		}
        // 	}
        // }

    }
}
