namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DirectoryServices.ActiveDirectory.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_DirectoryServices_ActiveDirectory_NativeMethodsImpl
	{

		public static System.Int32 DsGetDcName(System.String computerName, System.String domainName, System.IntPtr domainGuid, System.String siteName, System.Int32 flags, System.IntPtr* domainControllerInfo)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcName' has not been implemented!");
		}

		public static System.Int32 DsGetDcOpen(System.String dnsName, System.Int32 optionFlags, System.String siteName, System.IntPtr domainGuid, System.String dnsForestName, System.Int32 dcFlags, System.IntPtr* retGetDcContext)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcOpen' has not been implemented!");
		}

		public static System.Int32 DsGetDcNext(System.IntPtr getDcContextHandle, System.IntPtr* sockAddressCount, System.IntPtr* sockAdresses, System.IntPtr* dnsHostName)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcNext' has not been implemented!");
		}

		public static System.Void DsGetDcClose(System.IntPtr getDcContextHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcClose' has not been implemented!");
		}

		public static System.Int32 NetApiBufferFree(System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.NetApiBufferFree' has not been implemented!");
		}

		public static System.Int32 GetLastError()
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.GetLastError' has not been implemented!");
		}

		public static System.Int32 DnsQuery(System.String recordName, System.Int16 recordType, System.Int32 options, System.IntPtr servers, System.IntPtr* dnsResultList, System.IntPtr reserved)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DnsQuery' has not been implemented!");
		}

		public static System.Void DnsRecordListFree(System.IntPtr dnsResultList, System.Boolean dnsFreeType)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.DnsRecordListFree' has not been implemented!");
		}

		public static System.Boolean GetVersionEx(System.DirectoryServices.ActiveDirectory.OSVersionInfoEx ver)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.GetVersionEx' has not been implemented!");
		}

		public static System.Int32 LsaConnectUntrusted(System.DirectoryServices.ActiveDirectory.LsaLogonProcessSafeHandle* lsaHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.LsaConnectUntrusted' has not been implemented!");
		}

		public static System.Int32 LsaCallAuthenticationPackage(System.DirectoryServices.ActiveDirectory.LsaLogonProcessSafeHandle lsaHandle, System.Int32 authenticationPackage, System.DirectoryServices.ActiveDirectory.NegotiateCallerNameRequest protocolSubmitBuffer, System.Int32 submitBufferLength, System.IntPtr* protocolReturnBuffer, System.Int32* returnBufferLength, System.Int32* protocolStatus)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.LsaCallAuthenticationPackage' has not been implemented!");
		}

		public static System.UInt32 LsaFreeReturnBuffer(System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.LsaFreeReturnBuffer' has not been implemented!");
		}

		public static System.Int32 LsaDeregisterLogonProcess(System.IntPtr lsaHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.LsaDeregisterLogonProcess' has not been implemented!");
		}

		public static System.Int32 CompareString(System.UInt32 locale, System.UInt32 dwCmpFlags, System.IntPtr lpString1, System.Int32 cchCount1, System.IntPtr lpString2, System.Int32 cchCount2)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.NativeMethods.CompareString' has not been implemented!");
		}
	}
}
