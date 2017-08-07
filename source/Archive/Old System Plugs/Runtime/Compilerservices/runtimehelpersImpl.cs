using System;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;


namespace Cosmos.System.Plugs.System.Runtime.Compilerservices
{

    [Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
    public static class runtimehelpersImpl
    {
        public static int GetHashCode(object o)
        {
            throw new NotImplementedException("runtimehelpersImpl.GetHashCode()");
        }
    }
}