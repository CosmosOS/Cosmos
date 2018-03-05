using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    class CPUID : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            //Note that the arguments are pushed left to right. Thus, eaxoperation will be on the bottom of the stack.
            //Since the stack grows to 0, we need to put 24 to get the first arg.
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 24);
            //Call cpuid to get the information
            XS.Cpuid();

            //Now comes a trick to only use 4 general purpose registers (eax, ebx, ecx, edx)

            //Save the possible value ebx contains
            XS.Push(XSRegisters.EBX);
            //Set in ebx a pointer to the data (in this case, a pointer to "uint* eax", i.e, the second argument)
            XS.Set(XSRegisters.EBX, XSRegisters.EBP, sourceDisplacement: (20));
            //Exchange the eax and ebx registers.
            //Now eax has the pointer and ebx has the value returned by cpuid in eax
            XS.Exchange(XSRegisters.EAX, XSRegisters.EBX); 
            //Store the cpuid eax value in uint *eax, i.e, store the cpuid eax in the second argument
            XS.Set(XSRegisters.EAX, XSRegisters.EBX, destinationIsIndirect: true);
            //Set ebx as it were
            XS.Pop(XSRegisters.EBX);

            //Do the same strategy for all the rest
            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: (16));
            XS.Exchange(XSRegisters.EBX, XSRegisters.EAX); 
            XS.Set(XSRegisters.EBX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);

            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12);
            XS.Exchange(XSRegisters.ECX, XSRegisters.EAX);
            XS.Set(XSRegisters.ECX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);

            XS.Push(XSRegisters.EAX);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 8);
            XS.Exchange(XSRegisters.EDX, XSRegisters.EAX);
            XS.Set(XSRegisters.EDX, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Pop(XSRegisters.EAX);
        }
    }
}
