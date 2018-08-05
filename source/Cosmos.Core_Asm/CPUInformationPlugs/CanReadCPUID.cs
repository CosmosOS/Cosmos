using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm.CPUInformationPlugs
{
    class CanReadCPUID : AssemblerMethod
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
            XS.Xor(XSRegisters.ESP, 0x00200000, destinationIsIndirect: true);
            XS.Popfd();
            XS.Pushfd();
            XS.Pop(XSRegisters.EAX);
            XS.Xor(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);
            XS.Popfd();
            XS.And(XSRegisters.EAX, 0x00200000);
            XS.Set(XSRegisters.EAX, 1);
            XS.Push(XSRegisters.EAX);

        }
    }
}
