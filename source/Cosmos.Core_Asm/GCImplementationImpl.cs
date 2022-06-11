using Cosmos.Core_Asm.GCImplementation;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm;

[Plug(Target = typeof(Core.GCImplementation))]
public static class GCImplementationImpl
{
    [PlugMethod(Assembler = typeof(GetPointerAsm))]
    public static unsafe uint* GetPointer(object aObject) => throw null;
}
