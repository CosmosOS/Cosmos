namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeAxlBufferHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeAxlBufferHandleImpl
	{

		public static System.IntPtr GetProcessHeap()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeAxlBufferHandle.GetProcessHeap' has not been implemented!");
		}

		public static System.Boolean HeapFree(System.IntPtr hHeap, System.Int32 dwFlags, System.IntPtr lpMem)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeAxlBufferHandle.HeapFree' has not been implemented!");
		}
	}
}
