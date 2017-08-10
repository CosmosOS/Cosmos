using System;

using XSharp.Common;
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

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            ILOpCodes.OpSwitch OpSw = ( ILOpCodes.OpSwitch )aOpCode;
            XS.Pop(XSRegisters.EAX);
            for( int i = 0; i < OpSw.BranchLocations.Length; i++ )
            {
                XS.Compare(XSRegisters.EAX, ( uint )i);
                //string DestLabel = AssemblerNasm.TmpBranchLabel( aMethod, new ILOpCodes.OpBranch( ILOpCode.Code.Jmp, aOpCode.Position, OpSw.BranchLocations[ i ] ) );
                string xDestLabel = AppAssembler.TmpPosLabel(aMethod, OpSw.BranchLocations[i]);
                XS.Jump(CPUx86.ConditionalTestEnum.Equal, xDestLabel);
            }
        }


        // using System;
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Switch)]
        // 	public class Switch: Op {
        // 		private string[] Labels;
        // 		public Switch(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			uint[] xCases = aReader.OperandValueBranchLocations;
        // 			Labels = new string[xCases.Length];
        // 			for (int i = 0; i < xCases.Length; i++) {
        // 			    Labels[i] = GetInstructionLabel(xCases[i]);
        // 			}
        // 		}
        //
        // 		public override void DoAssemble() {
        //             XS.Pop(XSRegisters.EAX);
        // 			for(int i = 0; i < Labels.Length; i++){
        //                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue =(uint)i };
        //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = Labels[i] };
        // 			}
        // 			Assembler.Stack.Pop();
        // 		}
        // 	}
        // }

    }
}
