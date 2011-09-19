namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.UnsafeNclNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_UnsafeNclNativeMethodsImpl
	{

		public static System.IntPtr CreateSemaphore(System.IntPtr lpSemaphoreAttributes, System.Int32 lInitialCount, System.Int32 lMaximumCount, System.IntPtr lpName)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods.CreateSemaphore' has not been implemented!");
		}

		public static System.Boolean ReleaseSemaphore(System.IntPtr hSemaphore, System.Int32 lReleaseCount, System.IntPtr lpPreviousCount)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods.ReleaseSemaphore' has not been implemented!");
		}

		public static System.UInt32 GetCurrentThreadId()
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods.GetCurrentThreadId' has not been implemented!");
		}

		public static System.Void DebugBreak()
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods.DebugBreak' has not been implemented!");
		}
	}
}
