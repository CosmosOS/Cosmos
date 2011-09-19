namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.RegisteredWaitHandleSafe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_RegisteredWaitHandleSafeImpl
	{

		public static System.Void WaitHandleCleanupNative(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Threading.RegisteredWaitHandleSafe.WaitHandleCleanupNative' has not been implemented!");
		}

		public static System.Boolean UnregisterWaitNative(System.IntPtr handle, System.Runtime.InteropServices.SafeHandle waitObject)
		{
			throw new System.NotImplementedException("Method 'System.Threading.RegisteredWaitHandleSafe.UnregisterWaitNative' has not been implemented!");
		}
	}
}
