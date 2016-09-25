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
#warning I don't know if this is correctly plugged, but it's here to see other plugs missing for Zork to work
        public static bool Equals(ValueType aThis, object aThat)
        {
            return (aThis.GetType() == aThat.GetType()) && (aThis == aThat);
        }

        public static int GetHashCode(ValueType aThis)
        {
            if (aThis is byte)
                return (int)aThis;

            return -1;
        }

        public static int GetHashCodeOfPtr(IntPtr ptr)
        {
            throw new NotImplementedException("ValueType.GetHashCodeOfPtr()");
        }

        public static string ToString(ValueType aThis)
        {
            return "<ValueType.ToString not yet implemented!>";
        }
    }
}
