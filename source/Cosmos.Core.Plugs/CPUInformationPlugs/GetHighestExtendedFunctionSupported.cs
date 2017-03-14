using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs
{
    class GetHighestExtendedFunctionSupported : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            //This magic number returns the highest extended function supported in EAX
            XS.Set(XSRegisters.EAX, 0x80000000);
            XS.Cpuid();
            //Return the number as a parameter
            XS.Push(XSRegisters.EAX);
        }
    }
}
