using Cosmos.Core;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Multiboot))]
    public class MultibootImpl
    {
        [PlugMethod(Assembler = typeof(MultibootImplAsm))]
        public static uint GetMBIAddress()
        {
            return 0;
        }
    }

    public class MultibootImplAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Push("MultiBootInfo_Structure", isIndirect: true);
        }
    }
}
