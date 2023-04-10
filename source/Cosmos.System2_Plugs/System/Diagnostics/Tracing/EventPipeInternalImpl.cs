using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Diagnostics.Tracing
{
    [Plug("System.Diagnostics.Tracing.EventPipeInternal, System.Private.CoreLib")]
    class EventPipeInternalImpl
    {
        [PlugMethod(Signature = "System_Int32__System_Diagnostics_Tracing_EventPipeInternal_EventActivityIdControl_System_UInt32___System_Guid_")]
        public static int EventActivityIdControl(uint aUint, Guid aGuid)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_IntPtr__System_Diagnostics_Tracing_EventPipeInternal_CreateProvider_System_String__Interop_Advapi32_EtwEnableCallback_")]
        public static IntPtr CreateProvider(string aString, object aEtwEnableCallback)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Diagnostics_Tracing_EventPipeInternal_WriteEventData_System_IntPtr__System_Diagnostics_Tracing_EventProvider_EventData___System_UInt32__System_Guid___System_Guid__")]
        public static unsafe object WriteEventData(IntPtr aIntPtr, void* aEventData, uint aUint, Guid* aGuid, Guid* aGuidPtr2)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_IntPtr__System_Diagnostics_Tracing_EventPipeInternal_DefineEvent_System_IntPtr__System_UInt32__System_Int64__System_UInt32__System_UInt32__System_Void___System_UInt32_")]
        public static unsafe IntPtr DefineEvent(IntPtr aIntPtr, uint aUint, long aLong, uint aUint2, uint aUint3, void* aVoidPtr, uint aUint4)
        {
            throw new NotImplementedException();
        }

        public static void DeleteProvider(IntPtr aIntrPtr)
        {
            throw new NotImplementedException();
        }
    }
}