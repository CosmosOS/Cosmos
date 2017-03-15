using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;

namespace Cosmos.Core.Plugs.CPUInformationPlugs
{
    class RDMSR : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            //Set the operation specified on the argument to the ecx register
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 16);
            XS.Rdmsr();
            //Get a pointer to the *edx variable
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: 12);
            //Save the edx register in *edx
            XS.Set(XSRegisters.EBX, XSRegisters.EDX, destinationIsIndirect: true);
            //Do the same to eax
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX, destinationIsIndirect: true);
        }
    }
}
