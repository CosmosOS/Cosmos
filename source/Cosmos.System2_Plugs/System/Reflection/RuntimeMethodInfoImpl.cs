using System;
using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Reflection;

[Plug("System.Reflection.RuntimeMethodInfo, System.Private.CoreLib")]
internal class RuntimeMethodInfoImpl
{
    public static int get_MetadataToken(object aThis) => throw new NotImplementedException();

    public static string ToString(object aThis) => throw new NotImplementedException();

    public new static bool Equals(object aThis, object aOther) => throw new NotImplementedException();

    public static CallingConventions get_CallingConvention(object aThis) => throw new NotImplementedException();

    public static ParameterInfo[] GetParametersNoCopy(object aThis) => throw new NotImplementedException();

    public static ParameterInfo[] GetParameters(object aThis) => throw new NotImplementedException();
}
