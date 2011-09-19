namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.InteropServices.GCHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_InteropServices_GCHandleImpl
	{

		public static System.IntPtr InternalAlloc(System.Object value, System.Runtime.InteropServices.GCHandleType type)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalAlloc' has not been implemented!");
		}

		public static System.Void InternalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalFree' has not been implemented!");
		}

		public static System.Object InternalGet(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalGet' has not been implemented!");
		}

		public static System.Void InternalSet(System.IntPtr handle, System.Object value, System.Boolean isPinned)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalSet' has not been implemented!");
		}

		public static System.Object InternalCompareExchange(System.IntPtr handle, System.Object value, System.Object oldValue, System.Boolean isPinned)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalCompareExchange' has not been implemented!");
		}

		public static System.IntPtr InternalAddrOfPinnedObject(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalAddrOfPinnedObject' has not been implemented!");
		}

		public static System.Void InternalCheckDomain(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.InteropServices.GCHandle.InternalCheckDomain' has not been implemented!");
		}
	}
}
