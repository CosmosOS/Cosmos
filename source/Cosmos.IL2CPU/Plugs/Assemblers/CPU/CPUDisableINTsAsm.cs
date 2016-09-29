using XSharp.Compiler;

namespace Cosmos.IL2CPU.Plugs.Assemblers.CPU
{
    public class CPUDisableINTsAsm : AssemblerMethod
    {
        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearInterruptFlag();
        }
    }
}
