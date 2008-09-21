using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(Environment))]
    public static class EnvironmentImpl
    {
        [PlugMethod(Signature = "System_Environment_OSName__System_Environment_get_OSInfo__")]
        public static int get_OSName() { return 0x82; }

    }

}
