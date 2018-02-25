using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ValueType))]
    public static class ValueTypeImpl
    {
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

        public static bool Equals(ValueType aThis, object obj)
        {
            throw new NotImplementedException("ValueType.Equals()");
        }

        //public static string ToString(ValueType aThis)
        //{
        //    return "<ValueType.ToString not yet implemented!>";
        //}
    }
}
