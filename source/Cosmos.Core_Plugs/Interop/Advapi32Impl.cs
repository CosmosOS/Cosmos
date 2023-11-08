using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Advapi32, System.Private.CoreLib", IsOptional = true)]
    class Advapi32Impl
    {
        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_EventActivityIdControl_Interop_Advapi32_ActivityControl___System_Guid_")]
        public static int EventActivityIdControl(uint aUint, ref Guid aGuid)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_EventSetInformation_System_Int64__Interop_Advapi32_EVENT_INFO_CLASS__System_Void___System_Int32_")]
        public static unsafe int EventSetInformation(long aLong, int aEVENT_INFO_CLASS, void* aVoidPtr, int aInt)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_EnumerateTraceGuidsEx_Interop_Advapi32_TRACE_QUERY_INFO_CLASS__System_Void___System_Int32__System_Void___System_Int32___System_Int32_")]
        public static unsafe int EnumerateTraceGuidsEx(uint aTRACE_QUERY_INFO_CLASS, void* aVoidPtr, int aInt, void* aVoidPtr2, int aInt2, ref int aInt3)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegQueryValueEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_String__System_Int32_____System_Int32__System_Byte_____System_Int32_")]
        public static unsafe int RegQueryValueEx(object aSafeRegistryHandle, string aString, int[] aIntArray, ref int aInt, byte[] aByteArray, ref int aInt2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegQueryValueEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_String__System_Int32_____System_Int32___System_Int64___System_Int32_")]
        public static unsafe int RegQueryValueEx(object aSafeRegistryHandle, string aString, int[] aIntArray, ref int aInt, ref long aLong, ref int aInt2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegQueryValueEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_String__System_Int32_____System_Int32__System_Char_____System_Int32_")]
        public static unsafe int RegQueryValueEx(object aSafeRegistryHandle, string aString, int[] aIntArray, ref int aInt, char[] aCharArray, ref int aInt2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegQueryValueEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_String__System_Int32_____System_Int32___System_Int32___System_Int32_")]
        public static unsafe int RegQueryValueEx(object aSafeRegistryHandle, string aString, int[] aIntArray, ref int aInt, ref int aInt2, ref int aInt3)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_UInt32__Interop_Advapi32_EventRegister__System_Guid__Interop_Advapi32_EtwEnableCallback__System_Void____System_Int64_")]
        public static unsafe uint EventRegister(ref Guid aGuid, object aEtwEnableCallback, void* aVoidPtr, ref long aLong)
        {
            throw new NotImplementedException();
        }

        public static uint EventUnregister(long aLong)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegOpenKeyEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_String__System_Int32__System_Int32___Internal_Win32_SafeHandles_SafeRegistryHandle_")]
        public static unsafe int RegOpenKeyEx(object aSafeHandle, string lpSubKey, int ulOptions, int samDesired, out object hkResult)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_EventWriteTransfer_PInvoke_System_Int64___System_Diagnostics_Tracing_EventDescriptor__System_Guid___System_Guid___System_Int32__System_Diagnostics_Tracing_EventProvider_EventData__")]
        public static unsafe int EventWriteTransfer_PInvoke(long aLong, ref object aEventDescriptor, Guid* aGuidPtr,
            Guid* aGuidPtr2, int aInt, void* aEventData)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegEnumValue_Internal_Win32_SafeHandles_SafeRegistryHandle__System_Int32__System_Char_____System_Int32__System_IntPtr__System_Int32____System_Byte____System_Int32___")]
        public static unsafe int RegEnumValue(object aSafeRegistryHandle, int aInt, char[] aCharArray, ref int aInt2,
            IntPtr aIntPtr, int[] aIntArray, byte[] aByteArray, int[] aIntArray2)
        {
            throw new NotImplementedException();
        }

        public static int RegCloseKey(IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_RegEnumKeyEx_Internal_Win32_SafeHandles_SafeRegistryHandle__System_Int32__System_Char_____System_Int32__System_Int32____System_Char____System_Int32____System_Int64___")]
        public static int RegEnumKeyEx(object aSafeRegistryHandle, int aInt, char[] aCharArray, ref int aInt1, int[] aIntArray,
            char[] aCharArray1, int[] aIntArray1, long[] alongArray)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Advapi32_EventSetInformation_System_Int64__Interop_Advapi32_EVENT_INFO_CLASS__System_Void___System_UInt32_")]
        public static unsafe int EventSetInformation(long registrationHandle, object informationClass, void* eventInformation, uint informationLength)
        {
            throw new NotImplementedException();
        }
    }
}
