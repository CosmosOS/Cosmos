namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Management.IWbemClassObjectFreeThreaded), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Management_IWbemClassObjectFreeThreadedImpl
	{

		public static System.Runtime.InteropServices.ComTypes.IStream CreateStreamOnHGlobal(System.IntPtr hGlobal, System.Int32 fDeleteOnRelease)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.CreateStreamOnHGlobal' has not been implemented!");
		}

		public static System.IntPtr GetHGlobalFromStream(System.Runtime.InteropServices.ComTypes.IStream pstm)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.GetHGlobalFromStream' has not been implemented!");
		}

		public static System.IntPtr GlobalLock(System.IntPtr hGlobal)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.GlobalLock' has not been implemented!");
		}

		public static System.Int32 GlobalUnlock(System.IntPtr pData)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.GlobalUnlock' has not been implemented!");
		}

		public static System.Void CoMarshalInterface(System.Runtime.InteropServices.ComTypes.IStream pStm, System.Guid* riid, System.IntPtr Unk, System.UInt32 dwDestContext, System.IntPtr pvDestContext, System.UInt32 mshlflags)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.CoMarshalInterface' has not been implemented!");
		}

		public static System.IntPtr CoUnmarshalInterface(System.Runtime.InteropServices.ComTypes.IStream pStm, System.Guid* riid)
		{
			throw new System.NotImplementedException("Method 'System.Management.IWbemClassObjectFreeThreaded.CoUnmarshalInterface' has not been implemented!");
		}
	}
}
