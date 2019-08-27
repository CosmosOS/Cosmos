using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    public class CPUReadCPUIDAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // TODO Get the type parameter from EBP+8 and move it to EAX
            /*
             * mov eax, 16h
             * cpuid
             */
            // TODO The result of cpuid will be in EDX:EAX so it will need to be moved to the return value
            // Set ESI to EBP+8?
        }
    }
}
