using IL2CPU.API.Attribs;
using Cosmos.Core.Memory;
using Cosmos.Core;
using System;

namespace Cosmos.Core_Plugs.System.Runtime.Loader
{
    [Plug("System.Runtime.Loader.AssemblyLoadContext, System.Private.CoreLib")]
    public static unsafe class AssemblyLoadContextImpl
	{
        public static bool IsTracingEnabled()
        {
            return false;
        }

        public static bool TraceAssemblyResolveHandlerInvoked(string assemblyName, string handlerName, string resultAssemblyName, string resultAssemblyPath)
        {
            throw new NotImplementedException();
        }
	}
}
