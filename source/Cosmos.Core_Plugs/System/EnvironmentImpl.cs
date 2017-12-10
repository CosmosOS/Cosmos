using System;
using System.Collections.Generic;
using System.Text;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    // System.Private.CoreLib, internal
    [Plug(TargetName = "System.Environment, System.Private.CoreLib")]
    public static class InternalEnvironmentImpl
    {
        public static void CCtor()
        {
        }
    }

    // System.Runtime.Extensions, public
    [Plug(typeof(Environment))]
    public static class EnvironmentImpl
    {
        public static void CCtor()
        {
        }
    }
}
