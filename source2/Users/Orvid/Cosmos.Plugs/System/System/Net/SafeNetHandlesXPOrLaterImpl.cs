namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.SafeNetHandlesXPOrLater), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_UnsafeNclNativeMethods+SafeNetHandlesXPOrLaterImpl
	{

		public static System.Void freeaddrinfo(System.IntPtr info)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandlesXPOrLater.freeaddrinfo' has not been implemented!");
		}

		public static System.Int32 getaddrinfo(System.String nodename, System.String servicename, System.Net.AddressInfo* hints, System.Net.SafeFreeAddrInfo* handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandlesXPOrLater.getaddrinfo' has not been implemented!");
		}
	}
}
