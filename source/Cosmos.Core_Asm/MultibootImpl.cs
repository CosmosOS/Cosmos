using Cosmos.Core;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Multiboot2))]
    public class Multiboot2Impl
    {
        [PlugMethod(Assembler = typeof(Multiboot2ImplAsm))]
        public uint GetMBIAddress()
        {
            return 0;
        }
    }

    public class Multiboot2ImplAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Push("MultiBootInfo_Structure", isIndirect: true);
        }
    }
}
