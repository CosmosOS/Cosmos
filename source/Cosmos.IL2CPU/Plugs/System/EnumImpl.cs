using Cosmos.IL2CPU.Plugs;
using System;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.System
{
    [Plug(Target = typeof(Enum))]
    public static class EnumImpl
    {
    }
}