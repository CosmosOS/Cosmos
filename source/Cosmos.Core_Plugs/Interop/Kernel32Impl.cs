using IL2CPU.API.Attribs;
using System;
namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Kernel32, System.Private.CoreLib", IsOptional = true)]
    class Kernel32Impl
    {
        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_LCMapStringEx_System_String__System_UInt32__System_Char___System_Int32__System_Void___System_Int32__System_Void___System_Void___System_IntPtr_")]
        public static unsafe int LCMapStringEx(string aString, uint aUint, char* aChar, int aInt, object aObject, int aInt2, object aObject2,
            object aObject3, IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_CompareStringOrdinal_System_Char___System_Int32__System_Char___System_Int32__System_Boolean_")]
        public static unsafe int CompareStringOrdinal(char* aStrA, int aLengthA, char* aStrB, int aLengthB, bool aIgnoreCase)
        {
            if (aIgnoreCase)
            {
                throw new NotImplementedException();
            }
            if (aLengthA < aLengthB)
            {
                return -1;
            }
            else if (aLengthA > aLengthB)
            {
                return 1;
            }
            for (int i = 0; i < aLengthA; i++)
            {
                if (aStrA[i] < aStrB[i])
                {
                    return -1;
                }
                else if (aStrA[i] < aStrB[i])
                {
                    return 1;
                }
            }
            return 0;
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_CompareStringEx_System_Char___System_UInt32__System_Char___System_Int32__System_Char___System_Int32__System_Void___System_Void___System_IntPtr_")]
        public static unsafe int CompareStringEx(char* aChar, uint aUint, char* aChar1, int aInt, char* aChar2, int aInt2, object aObject,
            object aObject1, IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_FindNLSStringEx_System_Char___System_UInt32__System_Char___System_Int32__System_Char___System_Int32__System_Int32___System_Void___System_Void___System_IntPtr_")]
        public static unsafe int FindNLSStringEx(char* aCharPtr, uint aUint, char* aCharPtr2, int aInt, char* aCharPtr3, int aInt2, int* aIntPtr, void* aVoidPtr, void* aVoidPtr2, IntPtr aInPtr2)
        {
            throw new NotImplementedException();
        }

        public static IntPtr LocalFree(IntPtr aInt)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__Interop_Kernel32_QueryUnbiasedInterruptTime__System_UInt64_")]
        public static bool QueryUnbiasedInterruptTime(ulong aULong)
        {
            throw new NotImplementedException();
        }

        public static int GetCurrentProcessId()
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature ="Interop_BOOL__Interop_Kernel32_QueryPerformanceFrequency_System_Int64__")]
        public static unsafe bool QueryPerformanceFrequency(long* aLongPtr)
        {
            throw new NotImplementedException();
        }

        public static long VerSetConditionMask(ulong aUlong, uint aUint, byte aByte)
        {
            throw new NotImplementedException();
        }
        public static unsafe int GetCalendarInfoEx(string aString, uint aUint, IntPtr aIntPtr, uint aUint2, IntPtr aIntPtr1,
            int aInt, ref int aInt2)
        {
            throw new NotImplementedException();
        }

        public static unsafe IntPtr LocalAlloc(uint aUint, UIntPtr aUIntPtr)
        {
            throw new NotImplementedException();
        }

        public static unsafe uint ExpandEnvironmentStrings(string aString, ref char aChar, uint aUint)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_UInt32__Interop_Kernel32_GetDynamicTimeZoneInformation__Interop_Kernel32_TIME_DYNAMIC_ZONE_INFORMATION_")]
        public static unsafe uint GetDynamicTimeZoneInformation(ref object aTIME_DYNAMIC_ZONE_INFORMATION)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "Interop_BOOL__Interop_Kernel32_GetUserPreferredUILanguages_System_UInt32__System_UInt32___System_Char___System_UInt32__")]
        public static unsafe bool GetUserPreferredUILanguages(uint aUint, uint* aUintPtr, char* aCharPtr, uint* aUintPtr2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_GetCalendarInfoEx_System_String__System_UInt32__System_IntPtr__System_UInt32__System_IntPtr__System_Int32__System_IntPtr_")]
        public static unsafe int GetCalendarInfoEx(string aString, uint aUint, IntPtr aIntPtr, uint aUint2, IntPtr aIntPtr1, int aInt, IntPtr aIntPtr2) {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Boolean__Interop_Kernel32_CloseHandle_System_IntPtr_")]
        public static bool CloseHandle(IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        public static bool SetThreadErrorMode(
           uint dwNewMode,
           out uint lpOldMode)
        {
            //TODO
            lpOldMode = 0;
            return true;
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_WriteFile_System_Runtime_InteropServices_SafeHandle__System_Byte___System_Int32___System_Int32__System_Threading_NativeOverlapped__")]
        public static unsafe int WriteFile(global::System.Runtime.InteropServices.SafeHandle aSafeHandle, byte* aBytePtr, int aInt, ref int aRefInt, global::System.Threading.NativeOverlapped* aNativeOverlappedPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Int32__Interop_Kernel32_ReadFile_System_Runtime_InteropServices_SafeHandle__System_Byte___System_Int32___System_Int32__System_Threading_NativeOverlapped__")]
        public static unsafe int ReadFile(global::System.Runtime.InteropServices.SafeHandle aSafeHandle, byte* aBytePtr, int aInt, ref int aRefInt, global::System.Threading.NativeOverlapped* aNativeOverlappedPtr)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__Interop_Kernel32_GetSystemInfo__Interop_Kernel32_SYSTEM_INFO_")]
        public static unsafe void GetSystemInfo(object lpSystemInfo)
        {
            throw new NotImplementedException();
        }
    }
}
