using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm;

public class CPUDisableINTsAsm : AssemblerMethod
{
    public override void AssembleNew(Assembler aAssembler, object aMethodInfo) => XS.ClearInterruptFlag();
}
