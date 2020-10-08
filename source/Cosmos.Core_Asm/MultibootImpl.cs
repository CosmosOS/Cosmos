using Cosmos.Core;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(Multiboot))]
    public class MultibootImpl
    {
        [PlugMethod(Assembler = typeof(MultibootAsm))]
        public static uint GetMBIAddress() => throw null;
    }
}
