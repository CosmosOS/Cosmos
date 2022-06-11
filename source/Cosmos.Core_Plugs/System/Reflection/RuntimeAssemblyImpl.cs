using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection;

[Plug("System.Reflection.RuntimeAssembly, System.Private.CoreLib")]
internal class RuntimeAssemblyImpl
{
    [PlugMethod(Signature =
        "System_Void__System_Reflection_RuntimeAssembly_GetFullName_System_Runtime_CompilerServices_QCallAssembly__System_Runtime_CompilerServices_StringHandleOnStack_")]
    public static void GetFullName(object aQCallAssembly, object aStringHandleOnStack) =>
        throw new NotImplementedException();

    public static object[] GetCustomAttributes(object aThis, Type aType, bool aBool) =>
        throw new NotImplementedException();
}
