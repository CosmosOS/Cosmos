using IL2CPU.API.Attribs;
using System;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Sys, System.Private.CoreLib", IsOptional = true)]
    class SysImpl
    {
        [PlugMethod(Signature = "System_IntPtr__Interop_Sys_GetUnixNamePrivate__")]
        public static IntPtr GetUnixNamePrivate()
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Sys_Stat__System_Byte___Interop_Sys_FileStatus_")]
        public static int Stat(string path, out object output)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Sys_LStat__System_Byte___Interop_Sys_FileStatus_")]
        public static int LStat(string path, out object output)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "SystemPrivateCoreLibInterop_Error__Interop_Sys_ConvertErrorPlatformToPal_System_Int32_")]
        public static object ConvertErrorPlatformToPal(int platformErrno)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Sys_ConvertErrorPalToPlatform_SystemPrivateCoreLibInterop_Error_")]
        public static int ConvertErrorPalToPlatform(object error)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Byte#__Interop_Sys_StrErrorR_System_Int32__System_Byte#__System_Int32_")]
        public static unsafe byte* StrErrorR(int platformErrno, byte* buffer, int bufferSize)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Sys_CanGetHiddenFlag__")]
        public static int CanGetHiddenFlag()
        {
            return 0; //false
        }
    }
}
