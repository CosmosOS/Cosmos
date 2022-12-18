using IL2CPU.API.Attribs;
using System;
using System.IO;

namespace Cosmos.System_Plugs.Interop
{
    struct SECURITY_ATTRIBUTES
    {

    }

    [Plug("Interop+Kernel32, System.IO.FileSystem", IsOptional = true)]
    class Kernel32Impl
    {
        [PlugMethod(Signature = "System_Boolean__Interop_Kernel32_CreateDirectoryPrivate_System_String___Interop_Kernel32_SECURITY_ATTRIBUTES_")]
        public static bool CreateDirectoryPrivate(string aPath, SECURITY_ATTRIBUTES aSECURITY_ATTRIBUTES)
        {
            Directory.CreateDirectory(aPath);
            return true;
        }

        [PlugMethod(Signature = "System_Boolean__Interop_Kernel32_SetThreadErrorMode_System_UInt32___System_UInt32_")]
        public static bool SetThreadErrorMode(uint aUint, ref uint aUint2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_FormatMessage_System_Int32__System_IntPtr__System_UInt32__System_Int32__System_Void___System_Int32__System_IntPtr_")]
        public static unsafe int FormatMessage(int aInt, IntPtr aIntPtr, uint aUint, int aInt2, void* aPtr, int aInt3, IntPtr aIntPtr2)
        {
            throw new NotImplementedException();
        }
    }
}
