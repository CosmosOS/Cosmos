using System;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldloca)]
    public class Ldloca : ILOp
    {
        public Ldloca(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpVar = (OpVar) aOpCode;
            var xAddress = GetEBPOffsetForLocal(aMethod, xOpVar.Value);
            xAddress += (GetStackCountForLocal(aMethod, aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value]) - 1)*4;

            // xAddress contains full size of locals, excluding the actual local
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP));
            new CPUx86.Sub
            {
                DestinationReg = CPUx86.RegistersEnum.EAX,
                SourceValue = xAddress
            };
            new CPUx86.Push
            {
                DestinationReg = CPUx86.RegistersEnum.EAX,
            };
        }
    }
}
