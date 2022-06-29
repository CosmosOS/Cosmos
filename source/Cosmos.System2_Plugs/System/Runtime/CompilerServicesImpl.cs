using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Runtime
{
    [Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
    public static class RuntimeHelpersImpl
    {
        /* For RAT in Cosmos the size of Reference Types is 8 on x86 / 32 bit too...
         * String object header has two reference types so the data part is at offset 16
         */ 
        public static int get_OffsetToStringData()
        {
            return 16;
        }
    }
}
