namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IdentityModel.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IdentityModel_NativeMethodsImpl
	{

		public static System.Boolean LogonUser(System.String lpszUserName, System.String lpszDomain, System.String lpszPassword, System.UInt32 dwLogonType, System.UInt32 dwLogonProvider, System.IdentityModel.SafeCloseHandle* phToken)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LogonUser' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.IntPtr tokenHandle, System.UInt32 tokenInformationClass, System.IdentityModel.SafeHGlobalHandle tokenInformation, System.UInt32 tokenInformationLength, System.UInt32* returnLength)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.GetTokenInformation' has not been implemented!");
		}

		public static System.Boolean CryptAcquireContextW(System.IdentityModel.SafeProvHandle* phProv, System.String pszContainer, System.String pszProvider, System.UInt32 dwProvType, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptAcquireContextW' has not been implemented!");
		}

		public static System.Boolean CryptImportKey(System.IdentityModel.SafeProvHandle hProv, System.Void* pbData, System.UInt32 dwDataLen, System.IntPtr hPubKey, System.UInt32 dwFlags, System.IdentityModel.SafeKeyHandle* phKey)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptImportKey' has not been implemented!");
		}

		public static System.Boolean CryptGetKeyParam(System.IdentityModel.SafeKeyHandle phKey, System.UInt32 dwParam, System.IntPtr pbData, System.UInt32* dwDataLen, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptGetKeyParam' has not been implemented!");
		}

		public static System.Boolean CryptSetKeyParam(System.IdentityModel.SafeKeyHandle phKey, System.UInt32 dwParam, System.Void* pbData, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptSetKeyParam' has not been implemented!");
		}

		public static System.Boolean CryptEncrypt(System.IdentityModel.SafeKeyHandle phKey, System.IntPtr hHash, System.Boolean final, System.UInt32 dwFlags, System.Void* pbData, System.Int32* dwDataLen, System.Int32 dwBufLen)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptEncrypt' has not been implemented!");
		}

		public static System.Boolean CryptDecrypt(System.IdentityModel.SafeKeyHandle phKey, System.IntPtr hHash, System.Boolean final, System.UInt32 dwFlags, System.Void* pbData, System.Int32* dwDataLen)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptDecrypt' has not been implemented!");
		}

		public static System.Boolean CryptDestroyKey(System.IntPtr phKey)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptDestroyKey' has not been implemented!");
		}

		public static System.Boolean CryptReleaseContext(System.IntPtr hProv, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.CryptReleaseContext' has not been implemented!");
		}

		public static System.Boolean LookupPrivilegeValueW(System.String lpSystemName, System.String lpName, System.IdentityModel.LUID* Luid)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LookupPrivilegeValueW' has not been implemented!");
		}

		public static System.Boolean AdjustTokenPrivileges(System.IdentityModel.SafeCloseHandle tokenHandle, System.Boolean disableAllPrivileges, System.IdentityModel.TOKEN_PRIVILEGE* newState, System.UInt32 bufferLength, System.IdentityModel.TOKEN_PRIVILEGE* previousState, System.UInt32* returnLength)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.AdjustTokenPrivileges' has not been implemented!");
		}

		public static System.Boolean RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.RevertToSelf' has not been implemented!");
		}

		public static System.Boolean OpenProcessToken(System.IntPtr processToken, System.Security.Principal.TokenAccessLevels desiredAccess, System.IdentityModel.SafeCloseHandle* tokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.OpenProcessToken' has not been implemented!");
		}

		public static System.Boolean OpenThreadToken(System.IntPtr threadHandle, System.Security.Principal.TokenAccessLevels desiredAccess, System.Boolean openAsSelf, System.IdentityModel.SafeCloseHandle* tokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.OpenThreadToken' has not been implemented!");
		}

		public static System.IntPtr GetCurrentProcess()
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.GetCurrentProcess' has not been implemented!");
		}

		public static System.IntPtr GetCurrentThread()
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.GetCurrentThread' has not been implemented!");
		}

		public static System.Boolean DuplicateTokenEx(System.IdentityModel.SafeCloseHandle existingTokenHandle, System.Security.Principal.TokenAccessLevels desiredAccess, System.IntPtr tokenAttributes, System.IdentityModel.SECURITY_IMPERSONATION_LEVEL impersonationLevel, System.IdentityModel.TokenType tokenType, System.IdentityModel.SafeCloseHandle* duplicateTokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.DuplicateTokenEx' has not been implemented!");
		}

		public static System.Boolean SetThreadToken(System.IntPtr threadHandle, System.IdentityModel.SafeCloseHandle threadToken)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.SetThreadToken' has not been implemented!");
		}

		public static System.Int32 LsaRegisterLogonProcess(System.IdentityModel.UNICODE_INTPTR_STRING* logonProcessName, System.IdentityModel.SafeLsaLogonProcessHandle* lsaHandle, System.IntPtr* securityMode)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaRegisterLogonProcess' has not been implemented!");
		}

		public static System.Int32 LsaConnectUntrusted(System.IdentityModel.SafeLsaLogonProcessHandle* lsaHandle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaConnectUntrusted' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 status)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.Int32 LsaLookupAuthenticationPackage(System.IdentityModel.SafeLsaLogonProcessHandle lsaHandle, System.IdentityModel.UNICODE_INTPTR_STRING* packageName, System.UInt32* authenticationPackage)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaLookupAuthenticationPackage' has not been implemented!");
		}

		public static System.Boolean AllocateLocallyUniqueId(System.IdentityModel.LUID* Luid)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.AllocateLocallyUniqueId' has not been implemented!");
		}

		public static System.Int32 LsaFreeReturnBuffer(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaFreeReturnBuffer' has not been implemented!");
		}

		public static System.Int32 LsaLogonUser(System.IdentityModel.SafeLsaLogonProcessHandle LsaHandle, System.IdentityModel.UNICODE_INTPTR_STRING* OriginName, System.IdentityModel.SecurityLogonType LogonType, System.UInt32 AuthenticationPackage, System.IntPtr AuthenticationInformation, System.UInt32 AuthenticationInformationLength, System.IntPtr LocalGroups, System.IdentityModel.TOKEN_SOURCE* SourceContext, System.IdentityModel.SafeLsaReturnBufferHandle* ProfileBuffer, System.UInt32* ProfileBufferLength, System.IdentityModel.LUID* LogonId, System.IdentityModel.SafeCloseHandle* Token, System.IdentityModel.QUOTA_LIMITS* Quotas, System.Int32* SubStatus)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaLogonUser' has not been implemented!");
		}

		public static System.Int32 LsaDeregisterLogonProcess(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.LsaDeregisterLogonProcess' has not been implemented!");
		}

		public static System.UInt32 SspiPromptForCredentials(System.String pszTargetName, System.IdentityModel.CREDUI_INFO* pUiInfo, System.UInt32 dwAuthError, System.String pszPackage, System.IntPtr authIdentity, System.IntPtr* ppAuthIdentity, System.Boolean* pfSave, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.SspiPromptForCredentials' has not been implemented!");
		}

		public static System.Boolean SspiIsPromptingNeeded(System.UInt32 ErrorOrNtStatus)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.NativeMethods.SspiIsPromptingNeeded' has not been implemented!");
		}
	}
}
