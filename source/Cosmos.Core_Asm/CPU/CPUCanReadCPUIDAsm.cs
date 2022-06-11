using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Asm;

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
        XS.Pop(EAX);
        XS.Set(ECX, EAX, true);
        XS.Xor(EAX, 1 << 21, true);
        XS.Push(EAX);
        XS.Popfd();
        XS.Pushfd();
        XS.Pop(EAX);
        XS.Push(ECX);
        XS.Popfd();
        XS.Xor(EAX, ECX, true);
        XS.Push(EAX);
    }
}
