namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Data.Common.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Data_Common_SafeNativeMethodsImpl
	{

		public static System.Int32 GetCurrentProcessId()
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.GetCurrentProcessId' has not been implemented!");
		}

		public static System.Int32 ReleaseSemaphore(System.IntPtr handle, System.Int32 releaseCount, System.IntPtr previousCount)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.ReleaseSemaphore' has not been implemented!");
		}

		public static System.Int32 WaitForMultipleObjectsEx(System.UInt32 nCount, System.IntPtr lpHandles, System.Boolean bWaitAll, System.UInt32 dwMilliseconds, System.Boolean bAlertable)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.WaitForMultipleObjectsEx' has not been implemented!");
		}

		public static System.Int32 WaitForSingleObjectEx(System.IntPtr lpHandles, System.UInt32 dwMilliseconds, System.Boolean bAlertable)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.WaitForSingleObjectEx' has not been implemented!");
		}

		public static System.IntPtr LocalAlloc(System.Int32 flags, System.IntPtr countOfBytes)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.LocalAlloc' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Data.Common.SafeNativeMethods.LocalFree' has not been implemented!");
		}
	}
}
