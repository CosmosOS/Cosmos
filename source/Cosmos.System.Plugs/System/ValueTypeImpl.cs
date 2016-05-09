using Cosmos.IL2CPU.Plugs;
using System;

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
