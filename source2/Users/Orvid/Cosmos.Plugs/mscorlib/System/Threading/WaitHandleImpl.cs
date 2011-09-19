namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.WaitHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_WaitHandleImpl
	{

		public static System.Int32 WaitMultiple(System.Threading.WaitHandle[] waitHandles, System.Int32 millisecondsTimeout, System.Boolean exitContext, System.Boolean WaitAll)
		{
			throw new System.NotImplementedException("Method 'System.Threading.WaitHandle.WaitMultiple' has not been implemented!");
		}

		public static System.Int32 WaitOneNative(System.Runtime.InteropServices.SafeHandle waitableSafeHandle, System.UInt32 millisecondsTimeout, System.Boolean hasThreadAffinity, System.Boolean exitContext)
		{
			throw new System.NotImplementedException("Method 'System.Threading.WaitHandle.WaitOneNative' has not been implemented!");
		}

		public static System.Int32 SignalAndWaitOne(Microsoft.Win32.SafeHandles.SafeWaitHandle waitHandleToSignal, Microsoft.Win32.SafeHandles.SafeWaitHandle waitHandleToWaitOn, System.Int32 millisecondsTimeout, System.Boolean hasThreadAffinity, System.Boolean exitContext)
		{
			throw new System.NotImplementedException("Method 'System.Threading.WaitHandle.SignalAndWaitOne' has not been implemented!");
		}
	}
}
