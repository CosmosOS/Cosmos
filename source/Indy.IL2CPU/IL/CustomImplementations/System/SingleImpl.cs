using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
    [Plug(Target = typeof(Single))]
    public static class SingleImpl
    {
        public static string ToString(ref uint aThis)
        {
            return "this is a single, it needs converting to a string";
        }
    }

    }
