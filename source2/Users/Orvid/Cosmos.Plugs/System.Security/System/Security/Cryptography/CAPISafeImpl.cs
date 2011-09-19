namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.CAPISafe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_CAPI+CAPISafeImpl
	{

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.LocalFree' has not been implemented!");
		}

		public static System.Void ZeroMemory(System.IntPtr handle, System.UInt32 length)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.ZeroMemory' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 status)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.Security.Cryptography.SafeLibraryHandle hModule, System.String lpProcName)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.GetProcAddress' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeLocalAllocHandle LocalAlloc(System.UInt32 uFlags, System.IntPtr sizetdwBytes)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.LocalAlloc' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeLibraryHandle LoadLibrary(System.String lpFileName)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.LoadLibrary' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertCreateCertificateContext(System.UInt32 dwCertEncodingType, System.Security.Cryptography.SafeLocalAllocHandle pbCertEncoded, System.UInt32 cbCertEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertCreateCertificateContext' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertDuplicateCertificateContext(System.IntPtr pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertDuplicateCertificateContext' has not been implemented!");
		}

		public static System.Boolean CertFreeCertificateContext(System.IntPtr pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertFreeCertificateContext' has not been implemented!");
		}

		public static System.Boolean CertGetCertificateChain(System.IntPtr hChainEngine, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.Runtime.InteropServices.ComTypes.FILETIME* pTime, System.Security.Cryptography.SafeCertStoreHandle hAdditionalStore, System.Security.Cryptography.CAPI+CERT_CHAIN_PARA* pChainPara, System.UInt32 dwFlags, System.IntPtr pvReserved, System.Security.Cryptography.SafeCertChainHandle* ppChainContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertGetCertificateChain' has not been implemented!");
		}

		public static System.Boolean CertGetCertificateContextProperty(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwPropId, System.Security.Cryptography.SafeLocalAllocHandle pvData, System.UInt32* pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertGetCertificateContextProperty' has not been implemented!");
		}

		public static System.UInt32 CertGetPublicKeyLength(System.UInt32 dwCertEncodingType, System.IntPtr pPublicKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertGetPublicKeyLength' has not been implemented!");
		}

		public static System.UInt32 CertNameToStrW(System.UInt32 dwCertEncodingType, System.IntPtr pName, System.UInt32 dwStrType, System.Security.Cryptography.SafeLocalAllocHandle psz, System.UInt32 csz)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertNameToStrW' has not been implemented!");
		}

		public static System.Boolean CertVerifyCertificateChainPolicy(System.IntPtr pszPolicyOID, System.Security.Cryptography.SafeCertChainHandle pChainContext, System.Security.Cryptography.CAPI+CERT_CHAIN_POLICY_PARA* pPolicyPara, System.Security.Cryptography.CAPI+CERT_CHAIN_POLICY_STATUS* pPolicyStatus)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CertVerifyCertificateChainPolicy' has not been implemented!");
		}

		public static System.Boolean CryptAcquireCertificatePrivateKey(System.Security.Cryptography.SafeCertContextHandle pCert, System.UInt32 dwFlags, System.IntPtr pvReserved, System.Security.Cryptography.SafeCryptProvHandle* phCryptProv, System.UInt32* pdwKeySpec, System.Boolean* pfCallerFreeProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptAcquireCertificatePrivateKey' has not been implemented!");
		}

		public static System.Boolean CryptDecodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.IntPtr pbEncoded, System.UInt32 cbEncoded, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle pvStructInfo, System.IntPtr pcbStructInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptDecodeObject' has not been implemented!");
		}

		public static System.Boolean CryptDecodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.Byte[] pbEncoded, System.UInt32 cbEncoded, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle pvStructInfo, System.IntPtr pcbStructInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptDecodeObject' has not been implemented!");
		}

		public static System.Boolean CryptEncodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.IntPtr pvStructInfo, System.Security.Cryptography.SafeLocalAllocHandle pbEncoded, System.IntPtr pcbEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptEncodeObject' has not been implemented!");
		}

		public static System.Boolean CryptEncodeObject(System.UInt32 dwCertEncodingType, System.String lpszStructType, System.IntPtr pvStructInfo, System.Security.Cryptography.SafeLocalAllocHandle pbEncoded, System.IntPtr pcbEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptEncodeObject' has not been implemented!");
		}

		public static System.IntPtr CryptFindOIDInfo(System.UInt32 dwKeyType, System.IntPtr pvKey, System.UInt32 dwGroupId)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptFindOIDInfo' has not been implemented!");
		}

		public static System.IntPtr CryptFindOIDInfo(System.UInt32 dwKeyType, System.Security.Cryptography.SafeLocalAllocHandle pvKey, System.UInt32 dwGroupId)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptFindOIDInfo' has not been implemented!");
		}

		public static System.Boolean CryptGetProvParam(System.Security.Cryptography.SafeCryptProvHandle hProv, System.UInt32 dwParam, System.IntPtr pbData, System.IntPtr pdwDataLen, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptGetProvParam' has not been implemented!");
		}

		public static System.Boolean CryptMsgGetParam(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwParamType, System.UInt32 dwIndex, System.IntPtr pvData, System.IntPtr pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgGetParam' has not been implemented!");
		}

		public static System.Boolean CryptMsgGetParam(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwParamType, System.UInt32 dwIndex, System.Security.Cryptography.SafeLocalAllocHandle pvData, System.IntPtr pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgGetParam' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToDecode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr hCryptProv, System.IntPtr pRecipientInfo, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgOpenToDecode' has not been implemented!");
		}

		public static System.Boolean CryptMsgUpdate(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.Byte[] pbData, System.UInt32 cbData, System.Boolean fFinal)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgUpdate' has not been implemented!");
		}

		public static System.Boolean CryptMsgUpdate(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.IntPtr pbData, System.UInt32 cbData, System.Boolean fFinal)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgUpdate' has not been implemented!");
		}

		public static System.Boolean CryptMsgVerifyCountersignatureEncoded(System.IntPtr hCryptProv, System.UInt32 dwEncodingType, System.IntPtr pbSignerInfo, System.UInt32 cbSignerInfo, System.IntPtr pbSignerInfoCountersignature, System.UInt32 cbSignerInfoCountersignature, System.IntPtr pciCountersigner)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPISafe.CryptMsgVerifyCountersignatureEncoded' has not been implemented!");
		}
	}
}
