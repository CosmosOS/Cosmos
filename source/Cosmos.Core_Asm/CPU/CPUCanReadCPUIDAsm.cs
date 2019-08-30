using Cosmos.Debug.Kernel;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUCanReadCPUIDAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            /*
             * pushfd
             * pushfd
             * xor dword [esp], 00200000h
             * popfd
             * pushfd
             * pop eax
             * xor eax, [esp]
             * and eax, 00200000h
             * ret
             */
            XS.Pushfd();
            XS.Pushfd();
            XS.Xor(ESP, 0x00200000, destinationIsIndirect: true);
            XS.Popfd();
            XS.Pushfd();
            XS.Pop(EAX);
            XS.Xor(EAX, ESP, destinationIsIndirect: true);
            XS.Popfd();
            XS.And(EAX, 0x00200000);
            XS.Push(EAX);
        }
    }
}
