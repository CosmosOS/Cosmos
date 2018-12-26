using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.CompilerServices
{
    [Plug("System.Runtime.CompilerServices.JitHelpers, System.Private.CoreLib")]
    public static class JitHelpersImpl
    {
        [PlugMethod(Signature = "_System_Byte__System_Runtime_CompilerServices_JitHelpers_GetRawSzArrayData_System_Array_")]
        public unsafe static ref byte GetRawSzArrayData([ObjectPointerAccess] byte* array) => ref *(array + 16);
    }
}
