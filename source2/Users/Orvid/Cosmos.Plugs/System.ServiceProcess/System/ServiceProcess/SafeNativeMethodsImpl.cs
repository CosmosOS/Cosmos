namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceProcess.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceProcess_SafeNativeMethodsImpl
	{

		public static System.Boolean CloseServiceHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.CloseServiceHandle' has not been implemented!");
		}

		public static System.IntPtr OpenSCManager(System.String machineName, System.String databaseName, System.Int32 access)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.OpenSCManager' has not been implemented!");
		}

		public static System.Int32 LsaClose(System.IntPtr objectHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.LsaClose' has not been implemented!");
		}

		public static System.Int32 LsaFreeMemory(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.LsaFreeMemory' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 ntStatus)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.Boolean GetServiceKeyName(System.IntPtr SCMHandle, System.String displayName, System.Text.StringBuilder shortName, System.Int32* shortNameLength)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.GetServiceKeyName' has not been implemented!");
		}

		public static System.Boolean GetServiceDisplayName(System.IntPtr SCMHandle, System.String shortName, System.Text.StringBuilder displayName, System.Int32* displayNameLength)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.SafeNativeMethods.GetServiceDisplayName' has not been implemented!");
		}
	}
}
