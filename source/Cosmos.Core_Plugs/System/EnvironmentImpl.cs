using System;
using System.Collections.Generic;
using System.Text;

using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(TargetName = "System.Environment, System.Private.CoreLib")]
    public static class EnvironmentImpl
    {
        public static void CCtor()
        {
        }
    }
}
