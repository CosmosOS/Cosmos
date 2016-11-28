using CPUx86 = Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Endfinally)]
    public class Endfinally : ILOp
    {
        public Endfinally(Cosmos.Assembler.Assembler aAsmblr) : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            XS.DataMember(aMethod.MethodBase.GetFullName() + "_" + "LeaveAddress_" + aOpCode.CurrentExceptionHandler.HandlerOffset.ToString("X2"), 0);
            XS.Set(EAX, aMethod.MethodBase.GetFullName() + "_" + "LeaveAddress_" + aOpCode.CurrentExceptionHandler.HandlerOffset.ToString("X2"));
            new CPUx86.Jump { DestinationReg = EAX, DestinationIsIndirect = true };
        }
    }
}
