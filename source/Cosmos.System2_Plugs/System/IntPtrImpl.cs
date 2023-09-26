using IL2CPU.API.Attribs;
using IL2CPU.API;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(IntPtr))]
    public static class IntPtrImpl
    {
        //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
        public static string ToString(IntPtr aThis)
        {
            return aThis.ToInt64().ToString();
        }

        public static int GetHashCode(ref IntPtr aThis)
        {
            return (int)aThis;
        }
    }
}