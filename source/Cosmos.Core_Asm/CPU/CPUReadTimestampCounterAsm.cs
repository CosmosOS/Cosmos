using System;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    public class CPUReadTimestampCounterAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // TODO need to move result from EDX:EAX to return value. Set ESI to EBP+8?
            /*
            * push eax
            * push ecx
            * push edx
            * rdtsc
            * mov [esi+4], eax
            * mov [esi], edx
            * pop edx
            * pop ecx
            * pop eax
            * ret
            */

            //XS.Push(XSRegisters.EAX);
            //XS.Push(XSRegisters.ECX);
            //XS.Push(XSRegisters.EDX);
            //XS.Rdtsc();
            //XS.Set(XSRegisters.ESI, XSRegisters.EAX, destinationIsIndirect: true, destinationDisplacement: 4);
            //XS.Set(XSRegisters.ESI, XSRegisters.EDX, destinationIsIndirect: true);
            //XS.Pop(XSRegisters.EDX);
            //XS.Pop(XSRegisters.ECX);
            //XS.Pop(XSRegisters.EAX);
            //XS.Return();
        }
    }
}
