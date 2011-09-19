namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeEventLogWriteHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeEventLogWriteHandleImpl
	{

		public static System.Boolean DeregisterEventSource(System.IntPtr hEventLog)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeEventLogWriteHandle.DeregisterEventSource' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeEventLogWriteHandle RegisterEventSource(System.String uncServerName, System.String sourceName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeEventLogWriteHandle.RegisterEventSource' has not been implemented!");
		}
	}
}
