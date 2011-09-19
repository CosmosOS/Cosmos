namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.HostExecutionContextManager), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_HostExecutionContextManagerImpl
	{

		public static System.Boolean HostSecurityManagerPresent()
		{
			throw new System.NotImplementedException("Method 'System.Threading.HostExecutionContextManager.HostSecurityManagerPresent' has not been implemented!");
		}

		public static System.Int32 ReleaseHostSecurityContext(System.IntPtr context)
		{
			throw new System.NotImplementedException("Method 'System.Threading.HostExecutionContextManager.ReleaseHostSecurityContext' has not been implemented!");
		}

		public static System.Int32 CloneHostSecurityContext(System.Runtime.InteropServices.SafeHandle context, System.Runtime.InteropServices.SafeHandle clonedContext)
		{
			throw new System.NotImplementedException("Method 'System.Threading.HostExecutionContextManager.CloneHostSecurityContext' has not been implemented!");
		}

		public static System.Int32 SetHostSecurityContext(System.Runtime.InteropServices.SafeHandle context, System.Boolean fReturnPrevious, System.Runtime.InteropServices.SafeHandle prevContext)
		{
			throw new System.NotImplementedException("Method 'System.Threading.HostExecutionContextManager.SetHostSecurityContext' has not been implemented!");
		}

		public static System.Int32 CaptureHostSecurityContext(System.Runtime.InteropServices.SafeHandle capturedContext)
		{
			throw new System.NotImplementedException("Method 'System.Threading.HostExecutionContextManager.CaptureHostSecurityContext' has not been implemented!");
		}
	}
}
