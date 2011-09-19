namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.Interop.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_Interop_UnsafeNativeMethodsImpl
	{

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateWaitableTimer(System.IntPtr mustBeZero, System.Boolean manualReset, System.String timerName)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.CreateWaitableTimer' has not been implemented!");
		}

		public static System.Boolean SetWaitableTimer(Microsoft.Win32.SafeHandles.SafeWaitHandle handle, System.Int64* dueTime, System.Int32 period, System.IntPtr mustBeZero, System.IntPtr mustBeZeroAlso, System.Boolean resume)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.SetWaitableTimer' has not been implemented!");
		}

		public static System.UInt32 GetSystemTimeAdjustment(System.Int32* adjustment, System.UInt32* increment, System.UInt32* adjustmentDisabled)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.GetSystemTimeAdjustment' has not been implemented!");
		}

		public static System.Void GetSystemTimeAsFileTime(System.Int64* time)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.GetSystemTimeAsFileTime' has not been implemented!");
		}

		public static System.UInt32 EventRegister(System.Guid* providerId, System.Runtime.Interop.UnsafeNativeMethods+EtwEnableCallback enableCallback, System.Void* callbackContext, System.Int64* registrationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventRegister' has not been implemented!");
		}

		public static System.UInt32 EventUnregister(System.Int64 registrationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventUnregister' has not been implemented!");
		}

		public static System.Int32 QueryPerformanceCounter(System.Int64* time)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.QueryPerformanceCounter' has not been implemented!");
		}

		public static System.Boolean IsDebuggerPresent()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.IsDebuggerPresent' has not been implemented!");
		}

		public static System.Void DebugBreak()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.DebugBreak' has not been implemented!");
		}

		public static System.Void OutputDebugString(System.String lpOutputString)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.OutputDebugString' has not been implemented!");
		}

		public static System.UInt32 EventWrite(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor, System.UInt32 userDataCount, System.Runtime.Interop.UnsafeNativeMethods+EventData* userData)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventWrite' has not been implemented!");
		}

		public static System.UInt32 EventWriteTransfer(System.Int64 registrationHandle, System.Diagnostics.Eventing.EventDescriptor* eventDescriptor, System.Guid* activityId, System.Guid* relatedActivityId, System.UInt32 userDataCount, System.Runtime.Interop.UnsafeNativeMethods+EventData* userData)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventWriteTransfer' has not been implemented!");
		}

		public static System.UInt32 EventWriteString(System.Int64 registrationHandle, System.Byte level, System.Int64 keywords, System.Char* message)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventWriteString' has not been implemented!");
		}

		public static System.UInt32 EventActivityIdControl(System.Int32 ControlCode, System.Guid* ActivityId)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.EventActivityIdControl' has not been implemented!");
		}

		public static System.Boolean ReportEvent(System.Runtime.InteropServices.SafeHandle hEventLog, System.UInt16 type, System.UInt16 category, System.UInt32 eventID, System.Byte[] userSID, System.UInt16 numStrings, System.UInt32 dataLen, System.Runtime.InteropServices.HandleRef strings, System.Byte[] rawData)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.ReportEvent' has not been implemented!");
		}

		public static System.Runtime.Interop.SafeEventLogWriteHandle RegisterEventSource(System.String uncServerName, System.String sourceName)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.RegisterEventSource' has not been implemented!");
		}

		public static System.Boolean GetComputerNameEx(System.Runtime.ComputerNameFormat nameType, System.Text.StringBuilder lpBuffer, System.Int32* size)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Interop.UnsafeNativeMethods.GetComputerNameEx' has not been implemented!");
		}
	}
}
