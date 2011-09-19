namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Web.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Web_SafeNativeMethodsImpl
	{

		public static System.Int32 GetCurrentProcessId()
		{
			throw new System.NotImplementedException("Method 'System.Web.SafeNativeMethods.GetCurrentProcessId' has not been implemented!");
		}

		public static System.Int32 GetCurrentThreadId()
		{
			throw new System.NotImplementedException("Method 'System.Web.SafeNativeMethods.GetCurrentThreadId' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceCounter(System.Int64* lpPerformanceCount)
		{
			throw new System.NotImplementedException("Method 'System.Web.SafeNativeMethods.QueryPerformanceCounter' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceFrequency(System.Int64* lpFrequency)
		{
			throw new System.NotImplementedException("Method 'System.Web.SafeNativeMethods.QueryPerformanceFrequency' has not been implemented!");
		}
	}
}
