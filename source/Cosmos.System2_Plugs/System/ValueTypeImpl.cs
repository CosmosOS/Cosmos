using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ValueType))]
    public static class ValueTypeImpl
    {
        public static int GetHashCode(ValueType aThis)
        {
            return (int)aThis;
        }

        public static int GetHashCodeOfPtr(IntPtr ptr)
        {
            throw new NotImplementedException("ValueType.GetHashCodeOfPtr()");
        }

        [PlugMethod(Signature = "System_Boolean__System_ValueType_Equals_System_Object")]
        public static unsafe bool Equals(ValueType aThis, object obj) // value type is just a pointer
        {
            if (aThis == null && obj == null)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (aThis == null)
            {
                return false;
            }
            return aThis.GetHashCode() == obj.GetHashCode();
        }


        //public static string ToString(ValueType aThis)
        //{
        //    return "<ValueType.ToString not yet implemented!>";
        //}
    }
}
