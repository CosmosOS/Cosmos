using Cosmos.Core.Processing;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Mutex))]
    public static unsafe class MutexImpl
    {
        [PlugMethod(Assembler = typeof(MutexLockASM))]
        public static void MutexLock(int* mtx) { }
    }

    public class MutexLockASM : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            new LiteralAssemblerCode("lock_asm:");
            new LiteralAssemblerCode("mov eax, [esp + 8]");
            new LiteralAssemblerCode("mov ebx, 0");
            new LiteralAssemblerCode("lock bts[eax], ebx");
            new LiteralAssemblerCode("jc.spin_wait");
            new LiteralAssemblerCode("mov ebx, 1");
            new LiteralAssemblerCode("mov dword[eax], ebx");
            new LiteralAssemblerCode("jmp .finished");
            new LiteralAssemblerCode(".spin_wait:");
            new LiteralAssemblerCode("mov ebx, 1");
            new LiteralAssemblerCode("test dword[eax], ebx");
            new LiteralAssemblerCode("pause");
            new LiteralAssemblerCode("jnz.spin_wait");
            new LiteralAssemblerCode("jmp lock_asm");
            new LiteralAssemblerCode(".finished");
        }
    }
}
