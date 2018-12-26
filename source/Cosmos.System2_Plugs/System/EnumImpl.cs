using System;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Enum))]
    public static class EnumImpl
    {
        //		[PlugMethod(Signature = "System_Void__System_Enum__cctor__")]
        public static void Cctor()
        {
            //
        }

        public static bool Equals(Enum aThis, object aEquals)
        {
            throw new NotSupportedException("Enum.Equals not supported yet!");
        }

        public static string ToString(Enum aThis) => "<Enum.ToString> not implemented";

        public static string ToString(Enum aThis, string format) => aThis.ToString();

        public static int GetHashCode(Enum aThis)
        {
            throw new NotImplementedException("Enum.GetHashCode()");
        }
    }
}
