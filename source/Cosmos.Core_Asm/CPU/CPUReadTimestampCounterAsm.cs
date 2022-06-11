using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm;

public class CPUReadTimestampCounterAsm : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
    {
        XS.Rdtsc();
        XS.Push(EDX);
        XS.Push(EAX);
    }
}
