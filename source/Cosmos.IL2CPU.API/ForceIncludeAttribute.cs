using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.IL2CPU.API
{
    /// <summary>
    /// Use this attribute to force the inclusion of a class, struct or a method in the compiled code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false)]
    public class ForceIncludeAttribute : Attribute
    {
    }
}
