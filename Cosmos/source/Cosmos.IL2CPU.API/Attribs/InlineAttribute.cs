using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.IL2CPU.API
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class InlineAttribute : Attribute
    {
        /// <summary>
        /// This field currently does nothing, but is here for later use.
        /// </summary>
        public TargetPlatform TargetPlatform = TargetPlatform.x86;
    }

    /// <summary>
    /// This enum contains the possible target platforms,
    /// to eventually allow for selective inclusion of plugs,
    /// depending on the target platform.
    /// </summary>
    public enum TargetPlatform
    {
        x86,
        x64,
        IA64,
        ARM
    }
}
