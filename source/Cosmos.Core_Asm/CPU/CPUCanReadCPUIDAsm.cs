using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    public class CPUCanReadCPUIDAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // TODO Need to move the result from EAX to the return value
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
            //XS.Pushfd();
            //XS.Pushfd();
            //XS.Xor(XSRegisters.ESP, 0x00200000, destinationIsIndirect: true);
            //XS.Popfd();
            //XS.Pushfd();
            //XS.Pop(XSRegisters.EAX);
            //XS.Xor(XSRegisters.EAX, XSRegisters.ESP, destinationIsIndirect: true);
            //XS.Popfd();
            //XS.And(XSRegisters.EAX, 0x00200000);
            //XS.Return();
        }
    }
}
