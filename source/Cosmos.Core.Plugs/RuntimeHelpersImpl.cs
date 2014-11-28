using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.Runtime.CompilerServices;

namespace Cosmos.Core.Plugs
{
    [Plug(Target=typeof(RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {
        public static int get_OffsetToStringData()
        {
            return 16;
        }
    }
}
