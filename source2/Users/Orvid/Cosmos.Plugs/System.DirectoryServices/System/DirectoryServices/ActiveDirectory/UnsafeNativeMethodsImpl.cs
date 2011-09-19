namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_DirectoryServices_ActiveDirectory_UnsafeNativeMethodsImpl
	{

		public static System.Int32 FormatMessageW(System.Int32 dwFlags, System.Int32 lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.Int32 arguments)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.FormatMessageW' has not been implemented!");
		}

		public static System.Int32 LocalFree(System.IntPtr mem)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LocalFree' has not been implemented!");
		}

		public static System.Int32 ADsEncodeBinaryData(System.Byte[] data, System.Int32 length, System.IntPtr* result)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ADsEncodeBinaryData' has not been implemented!");
		}

		public static System.Boolean FreeADsMem(System.IntPtr pVoid)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.FreeADsMem' has not been implemented!");
		}

		public static System.Int32 DsGetSiteName(System.String dcName, System.IntPtr* ptr)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsGetSiteName' has not been implemented!");
		}

		public static System.Int32 DsEnumerateDomainTrustsW(System.String serverName, System.Int32 flags, System.IntPtr* domains, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsEnumerateDomainTrustsW' has not been implemented!");
		}

		public static System.Int32 NetApiBufferFree(System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.NetApiBufferFree' has not been implemented!");
		}

		public static System.Int32 LogonUserW(System.String lpszUsername, System.String lpszDomain, System.String lpszPassword, System.Int32 dwLogonType, System.Int32 dwLogonProvider, System.IntPtr* phToken)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LogonUserW' has not been implemented!");
		}

		public static System.Int32 ImpersonateLoggedOnUser(System.IntPtr hToken)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ImpersonateLoggedOnUser' has not been implemented!");
		}

		public static System.Int32 RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RevertToSelf' has not been implemented!");
		}

		public static System.Int32 ConvertSidToStringSidW(System.IntPtr pSid, System.IntPtr* stringSid)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ConvertSidToStringSidW' has not been implemented!");
		}

		public static System.Int32 ConvertStringSidToSidW(System.IntPtr stringSid, System.IntPtr* pSid)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ConvertStringSidToSidW' has not been implemented!");
		}

		public static System.Int32 LsaSetForestTrustInformation(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING target, System.IntPtr forestTrustInfo, System.Int32 checkOnly, System.IntPtr* collisionInfo)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaSetForestTrustInformation' has not been implemented!");
		}

		public static System.Int32 LsaOpenPolicy(System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING target, System.DirectoryServices.ActiveDirectory.LSA_OBJECT_ATTRIBUTES objectAttributes, System.Int32 access, System.IntPtr* handle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaOpenPolicy' has not been implemented!");
		}

		public static System.Int32 LsaClose(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaClose' has not been implemented!");
		}

		public static System.Int32 LsaQueryForestTrustInformation(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING target, System.IntPtr* ForestTrustInfo)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaQueryForestTrustInformation' has not been implemented!");
		}

		public static System.Int32 LsaQueryTrustedDomainInfoByName(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING trustedDomain, System.DirectoryServices.ActiveDirectory.TRUSTED_INFORMATION_CLASS infoClass, System.IntPtr* buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 status)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.Int32 LsaFreeMemory(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaFreeMemory' has not been implemented!");
		}

		public static System.Int32 LsaSetTrustedDomainInfoByName(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING trustedDomain, System.DirectoryServices.ActiveDirectory.TRUSTED_INFORMATION_CLASS infoClass, System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaSetTrustedDomainInfoByName' has not been implemented!");
		}

		public static System.Int32 LsaOpenTrustedDomainByName(System.DirectoryServices.ActiveDirectory.PolicySafeHandle policyHandle, System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING trustedDomain, System.Int32 access, System.IntPtr* trustedDomainHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaOpenTrustedDomainByName' has not been implemented!");
		}

		public static System.Int32 LsaDeleteTrustedDomain(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.IntPtr pSid)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaDeleteTrustedDomain' has not been implemented!");
		}

		public static System.Int32 I_NetLogonControl2(System.String serverName, System.Int32 FunctionCode, System.Int32 QueryLevel, System.IntPtr data, System.IntPtr* buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.I_NetLogonControl2' has not been implemented!");
		}

		public static System.Void GetSystemTimeAsFileTime(System.IntPtr fileTime)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetSystemTimeAsFileTime' has not been implemented!");
		}

		public static System.Int32 LsaQueryInformationPolicy(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.Int32 infoClass, System.IntPtr* buffer)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaQueryInformationPolicy' has not been implemented!");
		}

		public static System.Int32 LsaCreateTrustedDomainEx(System.DirectoryServices.ActiveDirectory.PolicySafeHandle handle, System.DirectoryServices.ActiveDirectory.TRUSTED_DOMAIN_INFORMATION_EX domainEx, System.DirectoryServices.ActiveDirectory.TRUSTED_DOMAIN_AUTH_INFORMATION authInfo, System.Int32 classInfo, System.IntPtr* domainHandle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaCreateTrustedDomainEx' has not been implemented!");
		}

		public static System.IntPtr OpenThread(System.UInt32 desiredAccess, System.Boolean inheirted, System.Int32 threadID)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.OpenThread' has not been implemented!");
		}

		public static System.Int32 GetCurrentThreadId()
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetCurrentThreadId' has not been implemented!");
		}

		public static System.Int32 ImpersonateAnonymousToken(System.IntPtr token)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ImpersonateAnonymousToken' has not been implemented!");
		}

		public static System.Int32 CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Int32 RtlInitUnicodeString(System.DirectoryServices.ActiveDirectory.LSA_UNICODE_STRING result, System.IntPtr s)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString' has not been implemented!");
		}

		public static System.IntPtr LoadLibrary(System.String name)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LoadLibrary' has not been implemented!");
		}

		public static System.UInt32 FreeLibrary(System.IntPtr libName)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.DirectoryServices.ActiveDirectory.LoadLibrarySafeHandle hModule, System.String entryPoint)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress' has not been implemented!");
		}
	}
}
