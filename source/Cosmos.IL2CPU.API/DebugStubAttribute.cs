using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.API
{
    /// <summary>
    /// Method Attribute used to disable the outputing of the call to the DebugStub in that method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DebugStubAttribute : Attribute
    {
        /// <summary>
        /// Set to <value>true</value> to disable DebugStub
        /// </summary>
        public bool Off = false;
    }
}
