using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.CPUInformationPlugs
{
    class GetCurrentTimeStampCounter : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Rdtsc();
            //Get the edx pointer
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: 8);
            //Store the value on the variable edx
            XS.Set(XSRegisters.EBX, XSRegisters.EDX, destinationIsIndirect: true);
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: 12);
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, destinationIsIndirect: true);
        }
    }
}
