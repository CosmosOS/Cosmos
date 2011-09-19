namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Channels.PeerWinsock), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Channels_PeerWinsockImpl
	{

		public static System.Int32 WSAIoctl(System.IntPtr socketHandle, System.Int32 ioControlCode, System.IntPtr inBuffer, System.Int32 inBufferSize, System.IntPtr outBuffer, System.Int32 outBufferSize, System.Int32* bytesTransferred, System.IntPtr overlapped, System.IntPtr completionRoutine)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.PeerWinsock.WSAIoctl' has not been implemented!");
		}
	}
}
