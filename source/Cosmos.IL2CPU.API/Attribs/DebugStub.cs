using System;

namespace Cosmos.IL2CPU.API.Attribs
{
    /// <summary>
    /// Method Attribute used to disable the outputing of the call to the DebugStub in that method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DebugStub : Attribute
    {
        /// <summary>
        /// Set to <value>true</value> to disable DebugStub
        /// </summary>
        public bool Off = false;
    }
}
