using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(typeof(HashCode))]
internal class HashCodeImpl
{
    public static uint GenerateGlobalSeed() => 0;
}
