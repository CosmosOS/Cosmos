using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(ValueType))]
    public static class ValueTypeImpl
    {
        public static string ToString(ValueType aThis)
        {
            return "<ValueType.ToString not yet implemented!>";
        }

    }

}
