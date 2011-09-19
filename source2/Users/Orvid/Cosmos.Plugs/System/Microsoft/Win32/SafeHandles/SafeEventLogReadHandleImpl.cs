namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeEventLogReadHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeEventLogReadHandleImpl
	{

		public static System.Boolean CloseEventLog(System.IntPtr hEventLog)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeEventLogReadHandle.CloseEventLog' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeEventLogReadHandle OpenEventLog(System.String UNCServerName, System.String sourceName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeEventLogReadHandle.OpenEventLog' has not been implemented!");
		}
	}
}
