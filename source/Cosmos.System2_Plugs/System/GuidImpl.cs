using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(Guid))]
public class GuidImpl
{
    public static Guid NewGuid() => new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}
