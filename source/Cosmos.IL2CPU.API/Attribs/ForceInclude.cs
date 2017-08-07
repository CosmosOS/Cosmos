using System;

namespace Cosmos.IL2CPU.API.Attribs
{
    /// <summary>
    /// Use this attribute to force the inclusion of a class, struct or a method in the compiled code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false)]
    public class ForceInclude : Attribute
    {
    }
}
