using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(TargetName = "System.RuntimeType")]
    public static class RuntimeTypePlug
    {
        public static string ToString(object aThis)
        {
            throw new NotImplementedException("RuntimeTypePlug.ToString()");
        }

    }

}
