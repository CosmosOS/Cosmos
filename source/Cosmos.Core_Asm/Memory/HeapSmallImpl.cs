using System;
using Cosmos.Core.Memory;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm.Memory;

[Plug(Target = typeof(HeapSmall))]
internal class HeapSmallImpl
{
    [PlugMethod(Assembler = typeof(GetStringTypeIDAsm))]
    public static uint GetStringTypeID() => throw new NotImplementedException();
}
