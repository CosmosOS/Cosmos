using Cosmos.Debug.Kernel;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm
{
    public class CPUCanReadCPUIDAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            /*
             * pushfd
             * pop eax
             * mov ecx, eax
             * xor eax, 1 << 21
             * push eax
             * popfd
             * pushfd
             * pop eax
             * push ecx
             * popfd
             * xor eax, ecx
             */
            XS.Pushfd();
            XS.Pop(RAX);
            XS.Set(RCX, RAX, destinationIsIndirect: true);
            XS.Xor(RAX, 1 << 21, destinationIsIndirect: true);
            XS.Push(RAX);
            XS.Popfd();
            XS.Pushfd();
            XS.Pop(RAX);
            XS.Push(RCX);
            XS.Popfd();
            XS.Xor(RAX, RCX, destinationIsIndirect: true);
            XS.Push(RAX);
        }
    }
}
