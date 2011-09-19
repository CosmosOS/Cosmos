namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.OverlappedData), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_OverlappedDataImpl
	{

		public static System.Void FreeNativeOverlapped(System.Threading.NativeOverlapped* nativeOverlappedPtr)
		{
			throw new System.NotImplementedException("Method 'System.Threading.OverlappedData.FreeNativeOverlapped' has not been implemented!");
		}

		public static System.Threading.OverlappedData GetOverlappedFromNative(System.Threading.NativeOverlapped* nativeOverlappedPtr)
		{
			throw new System.NotImplementedException("Method 'System.Threading.OverlappedData.GetOverlappedFromNative' has not been implemented!");
		}

		public static System.Void CheckVMForIOPacket(System.Threading.NativeOverlapped** pOVERLAP, System.UInt32* errorCode, System.UInt32* numBytes)
		{
			throw new System.NotImplementedException("Method 'System.Threading.OverlappedData.CheckVMForIOPacket' has not been implemented!");
		}

		public static System.Threading.NativeOverlapped* AllocateNativeOverlapped(System.Threading.OverlappedData aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.OverlappedData.AllocateNativeOverlapped' has not been implemented!");
		}
	}
}
