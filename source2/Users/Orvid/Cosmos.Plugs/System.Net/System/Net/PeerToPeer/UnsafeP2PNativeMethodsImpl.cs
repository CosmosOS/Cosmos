namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.PeerToPeer.UnsafeP2PNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_PeerToPeer_UnsafeP2PNativeMethodsImpl
	{

		public static System.Void PeerFreeData(System.IntPtr dataToFree)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerFreeData' has not been implemented!");
		}

		public static System.Int32 PeerPnrpGetCloudInfo(System.UInt32* pNumClouds, System.Net.PeerToPeer.SafePeerData* pArrayOfClouds)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpGetCloudInfo' has not been implemented!");
		}

		public static System.Int32 PeerPnrpStartup(System.UInt16 versionRequired)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpStartup' has not been implemented!");
		}

		public static System.Int32 PeerCreatePeerName(System.String identity, System.String classfier, System.Net.PeerToPeer.SafePeerData* peerName)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerCreatePeerName' has not been implemented!");
		}

		public static System.Int32 PeerIdentityGetDefault(System.Net.PeerToPeer.SafePeerData* defaultIdentity)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerIdentityGetDefault' has not been implemented!");
		}

		public static System.Int32 PeerNameToPeerHostName(System.String peerName, System.Net.PeerToPeer.SafePeerData* peerHostName)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerNameToPeerHostName' has not been implemented!");
		}

		public static System.Int32 PeerHostNameToPeerName(System.String peerHostName, System.Net.PeerToPeer.SafePeerData* peerName)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerHostNameToPeerName' has not been implemented!");
		}

		public static System.Int32 PeerPnrpRegister(System.String pcwzPeerName, System.Net.PeerToPeer.PEER_PNRP_REGISTRATION_INFO* registrationInfo, System.Net.PeerToPeer.SafePeerNameUnregister* handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpRegister' has not been implemented!");
		}

		public static System.Int32 PeerPnrpUnregister(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpUnregister' has not been implemented!");
		}

		public static System.Int32 PeerPnrpUpdateRegistration(System.Net.PeerToPeer.SafePeerNameUnregister hRegistration, System.Net.PeerToPeer.PEER_PNRP_REGISTRATION_INFO* registrationInfo)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpUpdateRegistration' has not been implemented!");
		}

		public static System.Int32 PeerPnrpResolve(System.String pcwzPeerNAme, System.String pcwzCloudName, System.UInt32* pcEndPoints, System.Net.PeerToPeer.SafePeerData* pEndPoints)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpResolve' has not been implemented!");
		}

		public static System.Int32 PeerPnrpStartResolve(System.String pcwzPeerNAme, System.String pcwzCloudName, System.UInt32 cEndPoints, Microsoft.Win32.SafeHandles.SafeWaitHandle hEvent, System.Net.PeerToPeer.SafePeerNameEndResolve* safePeerNameEndResolve)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpStartResolve' has not been implemented!");
		}

		public static System.Int32 PeerPnrpGetEndpoint(System.IntPtr Handle, System.Net.PeerToPeer.SafePeerData* pEndPoint)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpGetEndpoint' has not been implemented!");
		}

		public static System.Int32 PeerPnrpEndResolve(System.IntPtr Handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.PeerToPeer.UnsafeP2PNativeMethods.PeerPnrpEndResolve' has not been implemented!");
		}
	}
}
