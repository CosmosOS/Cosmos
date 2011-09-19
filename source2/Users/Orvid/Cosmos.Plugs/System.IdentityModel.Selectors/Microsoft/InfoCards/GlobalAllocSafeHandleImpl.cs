namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.InfoCards.GlobalAllocSafeHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_InfoCards_GlobalAllocSafeHandleImpl
	{

		public static System.Void ZeroMemory(System.IntPtr dest, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'Microsoft.InfoCards.GlobalAllocSafeHandle.ZeroMemory' has not been implemented!");
		}

		public static System.IntPtr GlobalFree(System.IntPtr hMem)
		{
			throw new System.NotImplementedException("Method 'Microsoft.InfoCards.GlobalAllocSafeHandle.GlobalFree' has not been implemented!");
		}
	}
}
