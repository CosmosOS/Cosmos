using System;
using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System;

[Plug("System.RuntimeType, System.Private.CoreLib")]
public static class RuntimeTypeImpl
{
    [PlugMethod(Signature = "System_RuntimeType_RuntimeTypeCache__System_RuntimeType_get_Cache__")]
    public static IntPtr get_Cache(object aThis) => throw new NotSupportedException("Reflection not supported");

    public static string get_Name(object aThis) => "**Reflection not supported yet**";

    [PlugMethod(Signature = "System_Void__System_RuntimeType__cctor__")]
    public static void CCtor()
    {
        //
    }

    public static Type[] GetInterfaces(object aThis) => throw new NotImplementedException();

    public static FieldInfo[] GetFields(object aThis, BindingFlags aBindingFlags) =>
        throw new NotImplementedException();
}
