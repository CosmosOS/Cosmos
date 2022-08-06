using IL2CPU.API.Attribs;
using XSharp.Assembler;

namespace Cosmos.Core_Plugs.MemoryOperations
{
    [Plug(Target = typeof(Cosmos.Core.MemoryOperations))]
    public unsafe class MemoryOperationsImpl
    {
        [PlugMethod(Assembler = typeof(AsmCopyBytes))]
        public static void Copy(byte* dst, byte* src, int len) { }
        
        public class AsmCopyBytes : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                _ = new LiteralAssemblerCode("mov esi, [esp+12]");
                _ = new LiteralAssemblerCode("mov edi, [esp+16]");
                _ = new LiteralAssemblerCode("cld");
                _ = new LiteralAssemblerCode("mov ecx, [esp+8]");
                _ = new LiteralAssemblerCode("rep movsb");
            }
        }

        [PlugMethod(Assembler = typeof(AsmCopyUint))]
        public static void Copy(uint* dst, uint* src, int len) { }

        public class AsmCopyUint : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                _ = new LiteralAssemblerCode("mov esi, [esp+12]");
                _ = new LiteralAssemblerCode("mov edi, [esp+16]");
                _ = new LiteralAssemblerCode("cld");
                _ = new LiteralAssemblerCode("mov ecx, [esp+8]");
                _ = new LiteralAssemblerCode("rep movsd");
            }
        }

        [PlugMethod(Assembler = typeof(AsmSetByte))]
        public static void Fill(byte* dst, byte value, uint len) { }

        public class AsmSetByte : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                _ = new LiteralAssemblerCode("mov al, [esp+12]");
                _ = new LiteralAssemblerCode("mov edi, [esp+16]");
                _ = new LiteralAssemblerCode("cld");
                _ = new LiteralAssemblerCode("mov ecx, [esp+8]");
                _ = new LiteralAssemblerCode("rep stosb");
            }
        }

        [PlugMethod(Assembler = typeof(AsmSetUint))]
        public static void Fill(uint* dst, uint value, uint len) { }

        public class AsmSetUint : AssemblerMethod
        {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
            {
                _ = new LiteralAssemblerCode("mov eax, [esp+12]");
                _ = new LiteralAssemblerCode("mov edi, [esp+16]");
                _ = new LiteralAssemblerCode("cld");
                _ = new LiteralAssemblerCode("mov ecx, [esp+8]");
                _ = new LiteralAssemblerCode("rep stosd");
            }
        }
    }
}
