using XSharp.Compiler;

namespace Cosmos.IL2CPU.Plugs.Assemblers.CPU
{
    public class CPUGetEndOfKernelAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Push("_end_code");
        }
    }
}
