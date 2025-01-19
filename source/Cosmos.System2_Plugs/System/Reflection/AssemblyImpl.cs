using System;
using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Reflection
{
    [Plug(Target = typeof(Assembly))]
    class AssemblyImpl
    {
        public static object[] GetCustomAttributes(Assembly aThis, Type aType, bool aBool)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Reflection_Assembly_GetEntryAssemblyNative_System_Runtime_CompilerServices_ObjectHandleOnStack_")]
        public static void GetEntryAssemblyNative(object handleOnStack)
        {
            throw new NotImplementedException();
        }
    }
}