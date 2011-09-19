namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.NetworkInformation.UnsafeNetInfoNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_NetworkInformation_UnsafeNetInfoNativeMethodsImpl
	{

		public static System.UInt32 GetAdaptersAddresses(System.Net.Sockets.AddressFamily family, System.UInt32 flags, System.IntPtr pReserved, System.Net.SafeLocalFree adapterAddresses, System.UInt32* outBufLen)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetAdaptersAddresses' has not been implemented!");
		}

		public static System.UInt32 GetNetworkParams(System.Net.SafeLocalFree pFixedInfo, System.UInt32* pOutBufLen)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetNetworkParams' has not been implemented!");
		}

		public static System.Void FreeMibTable(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.FreeMibTable' has not been implemented!");
		}

		public static System.UInt32 CancelMibChangeNotify2(System.IntPtr notificationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.CancelMibChangeNotify2' has not been implemented!");
		}

		public static System.UInt32 GetAdaptersInfo(System.Net.SafeLocalFree pAdapterInfo, System.UInt32* pOutBufLen)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetAdaptersInfo' has not been implemented!");
		}

		public static System.UInt32 GetBestInterface(System.Int32 ipAddress, System.Int32* index)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetBestInterface' has not been implemented!");
		}

		public static System.UInt32 GetIfEntry(System.Net.NetworkInformation.MibIfRow* pIfRow)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetIfEntry' has not been implemented!");
		}

		public static System.UInt32 GetIpStatistics(System.Net.NetworkInformation.MibIpStats* statistics)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetIpStatistics' has not been implemented!");
		}

		public static System.UInt32 GetIpStatisticsEx(System.Net.NetworkInformation.MibIpStats* statistics, System.Net.Sockets.AddressFamily family)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetIpStatisticsEx' has not been implemented!");
		}

		public static System.UInt32 GetTcpStatistics(System.Net.NetworkInformation.MibTcpStats* statistics)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetTcpStatistics' has not been implemented!");
		}

		public static System.UInt32 GetTcpStatisticsEx(System.Net.NetworkInformation.MibTcpStats* statistics, System.Net.Sockets.AddressFamily family)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetTcpStatisticsEx' has not been implemented!");
		}

		public static System.UInt32 GetUdpStatistics(System.Net.NetworkInformation.MibUdpStats* statistics)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetUdpStatistics' has not been implemented!");
		}

		public static System.UInt32 GetUdpStatisticsEx(System.Net.NetworkInformation.MibUdpStats* statistics, System.Net.Sockets.AddressFamily family)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetUdpStatisticsEx' has not been implemented!");
		}

		public static System.UInt32 GetIcmpStatistics(System.Net.NetworkInformation.MibIcmpInfo* statistics)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetIcmpStatistics' has not been implemented!");
		}

		public static System.UInt32 GetIcmpStatisticsEx(System.Net.NetworkInformation.MibIcmpInfoEx* statistics, System.Net.Sockets.AddressFamily family)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetIcmpStatisticsEx' has not been implemented!");
		}

		public static System.UInt32 GetTcpTable(System.Net.SafeLocalFree pTcpTable, System.UInt32* dwOutBufLen, System.Boolean order)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetTcpTable' has not been implemented!");
		}

		public static System.UInt32 GetExtendedTcpTable(System.Net.SafeLocalFree pTcpTable, System.UInt32* dwOutBufLen, System.Boolean order, System.UInt32 IPVersion, System.Net.NetworkInformation.TcpTableClass tableClass, System.UInt32 reserved)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetExtendedTcpTable' has not been implemented!");
		}

		public static System.UInt32 GetUdpTable(System.Net.SafeLocalFree pUdpTable, System.UInt32* dwOutBufLen, System.Boolean order)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetUdpTable' has not been implemented!");
		}

		public static System.UInt32 GetExtendedUdpTable(System.Net.SafeLocalFree pUdpTable, System.UInt32* dwOutBufLen, System.Boolean order, System.UInt32 IPVersion, System.Net.NetworkInformation.UdpTableClass tableClass, System.UInt32 reserved)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetExtendedUdpTable' has not been implemented!");
		}

		public static System.UInt32 GetPerAdapterInfo(System.UInt32 IfIndex, System.Net.SafeLocalFree pPerAdapterInfo, System.UInt32* pOutBufLen)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.GetPerAdapterInfo' has not been implemented!");
		}

		public static System.Net.SafeCloseIcmpHandle IcmpCreateFile()
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.IcmpCreateFile' has not been implemented!");
		}

		public static System.Net.SafeCloseIcmpHandle Icmp6CreateFile()
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.Icmp6CreateFile' has not been implemented!");
		}

		public static System.Boolean IcmpCloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.IcmpCloseHandle' has not been implemented!");
		}

		public static System.UInt32 IcmpSendEcho2(System.Net.SafeCloseIcmpHandle icmpHandle, Microsoft.Win32.SafeHandles.SafeWaitHandle Event, System.IntPtr apcRoutine, System.IntPtr apcContext, System.UInt32 ipAddress, System.Net.SafeLocalFree data, System.UInt16 dataSize, System.Net.NetworkInformation.IPOptions* options, System.Net.SafeLocalFree replyBuffer, System.UInt32 replySize, System.UInt32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.IcmpSendEcho2' has not been implemented!");
		}

		public static System.UInt32 IcmpSendEcho2(System.Net.SafeCloseIcmpHandle icmpHandle, System.IntPtr Event, System.IntPtr apcRoutine, System.IntPtr apcContext, System.UInt32 ipAddress, System.Net.SafeLocalFree data, System.UInt16 dataSize, System.Net.NetworkInformation.IPOptions* options, System.Net.SafeLocalFree replyBuffer, System.UInt32 replySize, System.UInt32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.IcmpSendEcho2' has not been implemented!");
		}

		public static System.UInt32 Icmp6SendEcho2(System.Net.SafeCloseIcmpHandle icmpHandle, Microsoft.Win32.SafeHandles.SafeWaitHandle Event, System.IntPtr apcRoutine, System.IntPtr apcContext, System.Byte[] sourceSocketAddress, System.Byte[] destSocketAddress, System.Net.SafeLocalFree data, System.UInt16 dataSize, System.Net.NetworkInformation.IPOptions* options, System.Net.SafeLocalFree replyBuffer, System.UInt32 replySize, System.UInt32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.Icmp6SendEcho2' has not been implemented!");
		}

		public static System.UInt32 Icmp6SendEcho2(System.Net.SafeCloseIcmpHandle icmpHandle, System.IntPtr Event, System.IntPtr apcRoutine, System.IntPtr apcContext, System.Byte[] sourceSocketAddress, System.Byte[] destSocketAddress, System.Net.SafeLocalFree data, System.UInt16 dataSize, System.Net.NetworkInformation.IPOptions* options, System.Net.SafeLocalFree replyBuffer, System.UInt32 replySize, System.UInt32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.Icmp6SendEcho2' has not been implemented!");
		}

		public static System.UInt32 IcmpParseReplies(System.IntPtr replyBuffer, System.UInt32 replySize)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.IcmpParseReplies' has not been implemented!");
		}

		public static System.UInt32 Icmp6ParseReplies(System.IntPtr replyBuffer, System.UInt32 replySize)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.Icmp6ParseReplies' has not been implemented!");
		}

		public static System.UInt32 NotifyStableUnicastIpAddressTable(System.Net.Sockets.AddressFamily addressFamily, System.Net.NetworkInformation.SafeFreeMibTable* table, System.Net.NetworkInformation.StableUnicastIpAddressTableDelegate callback, System.IntPtr context, System.Net.NetworkInformation.SafeCancelMibChangeNotify* notificationHandle)
		{
			throw new System.NotImplementedException("Method 'System.Net.NetworkInformation.UnsafeNetInfoNativeMethods.NotifyStableUnicastIpAddressTable' has not been implemented!");
		}
	}
}
