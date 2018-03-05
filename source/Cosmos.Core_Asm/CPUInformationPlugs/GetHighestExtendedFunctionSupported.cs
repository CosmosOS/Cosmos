using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    class GetHighestExtendedFunctionSupported : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            //This magic number returns the highest extended function supported in EAX
            XS.Set(XSRegisters.EAX, 0x80000000);
            XS.Cpuid();
            //Return the number as a parameter
            XS.Push(XSRegisters.EAX);
        }
    }
}
