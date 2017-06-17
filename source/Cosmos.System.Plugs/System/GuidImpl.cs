using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Guid))]
    public class GuidImpl
    {
        public static Guid NewGuid()
        {
            return new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
