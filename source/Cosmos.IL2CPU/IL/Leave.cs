using System.Reflection;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Leave)]
    public class Leave : ILOp
    {
        public Leave(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            // apparently, Roslyn changed something to the output. We now have to figure out where to jump to.
            if (aOpCode.CurrentExceptionHandler.Flags.HasFlag(ExceptionHandlingClauseOptions.Finally)
              && aOpCode.CurrentExceptionHandler.HandlerOffset > aOpCode.Position)
            {
                XS.Set(aMethod.MethodBase.GetFullName() + "_" + "LeaveAddress_" + aOpCode.CurrentExceptionHandler.HandlerOffset.ToString("X2"), Assembler.CurrentIlLabel + "." + (Assembler.AsmIlIdx + 2).ToString("X2"), destinationIsIndirect: true, size: RegisterSize.Int32);
                XS.Jump(AppAssembler.TmpPosLabel(aMethod, aOpCode.CurrentExceptionHandler.HandlerOffset));
                //new CPUx86.Jump {DestinationLabel = AppAssembler.TmpPosLabel(aMethod, aOpCode.CurrentExceptionHandler.HandlerOffset + aOpCode.CurrentExceptionHandler.HandlerLength) };
            }

            XS.Jump(AppAssembler.TmpBranchLabel(aMethod, aOpCode));
            //new CPUx86.Jump {DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode)};
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
        //         XS.Jump(TargetLabel);
        // 		}
        // 	}
        // }

    }
}
