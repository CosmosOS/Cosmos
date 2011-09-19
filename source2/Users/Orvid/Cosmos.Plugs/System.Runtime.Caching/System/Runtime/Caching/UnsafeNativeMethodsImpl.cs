namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.Caching.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_Caching_UnsafeNativeMethodsImpl
	{

		public static System.Int32 RegCloseKey(System.IntPtr hKey)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Caching.UnsafeNativeMethods.RegCloseKey' has not been implemented!");
		}

		public static System.Int32 GetModuleFileName(System.IntPtr module, System.Text.StringBuilder filename, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Caching.UnsafeNativeMethods.GetModuleFileName' has not been implemented!");
		}

		public static System.Int32 GlobalMemoryStatusEx(System.Runtime.Caching.MEMORYSTATUSEX* memoryStatusEx)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Caching.UnsafeNativeMethods.GlobalMemoryStatusEx' has not been implemented!");
		}
	}
}
