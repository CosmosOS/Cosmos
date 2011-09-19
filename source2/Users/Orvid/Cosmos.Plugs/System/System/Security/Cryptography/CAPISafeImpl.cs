namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.CAPISafe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_CAPISafeImpl
	{

		public static System.UInt32 FormatMessage(System.UInt32 dwFlags, System.IntPtr lpSource, System.UInt32 dwMessageId, System.UInt32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.UInt32 nSize, System.IntPtr Arguments)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.FormatMessage' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeLocalAllocHandle LocalAlloc(System.UInt32 uFlags, System.IntPtr sizetdwBytes)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.LocalAlloc' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertDuplicateCertificateContext(System.IntPtr pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertDuplicateCertificateContext' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertChainHandle CertDuplicateCertificateChain(System.Security.Cryptography.SafeCertChainHandle pChainContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertDuplicateCertificateChain' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertStoreHandle CertDuplicateStore(System.IntPtr hCertStore)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertDuplicateStore' has not been implemented!");
		}

		public static System.IntPtr CertFindExtension(System.String pszObjId, System.UInt32 cExtensions, System.IntPtr rgExtensions)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertFindExtension' has not been implemented!");
		}

		public static System.Boolean CertGetCertificateChain(System.IntPtr hChainEngine, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.Runtime.InteropServices.ComTypes.FILETIME* pTime, System.Security.Cryptography.SafeCertStoreHandle hAdditionalStore, System.Security.Cryptography.CAPIBase+CERT_CHAIN_PARA* pChainPara, System.UInt32 dwFlags, System.IntPtr pvReserved, System.Security.Cryptography.SafeCertChainHandle* ppChainContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetCertificateChain' has not been implemented!");
		}

		public static System.Boolean CertGetCertificateContextProperty(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwPropId, System.Security.Cryptography.SafeLocalAllocHandle pvData, System.UInt32* pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetCertificateContextProperty' has not been implemented!");
		}

		public static System.UInt32 CertGetNameStringW(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwType, System.UInt32 dwFlags, System.IntPtr pvTypePara, System.Security.Cryptography.SafeLocalAllocHandle pszNameString, System.UInt32 cchNameString)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetNameStringW' has not been implemented!");
		}

		public static System.UInt32 CertNameToStrW(System.UInt32 dwCertEncodingType, System.IntPtr pName, System.UInt32 dwStrType, System.Security.Cryptography.SafeLocalAllocHandle psz, System.UInt32 csz)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertNameToStrW' has not been implemented!");
		}

		public static System.Boolean CertVerifyCertificateChainPolicy(System.IntPtr pszPolicyOID, System.Security.Cryptography.SafeCertChainHandle pChainContext, System.Security.Cryptography.CAPIBase+CERT_CHAIN_POLICY_PARA* pPolicyPara, System.Security.Cryptography.CAPIBase+CERT_CHAIN_POLICY_STATUS* pPolicyStatus)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertVerifyCertificateChainPolicy' has not been implemented!");
		}

		public static System.IntPtr CryptFindOIDInfo(System.UInt32 dwKeyType, System.Security.Cryptography.SafeLocalAllocHandle pvKey, System.Security.Cryptography.OidGroup dwGroupId)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptFindOIDInfo' has not been implemented!");
		}

		public static System.Void SetLastError(System.UInt32 dwErrorCode)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.SetLastError' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.LocalFree' has not been implemented!");
		}

		public static System.Void ZeroMemory(System.IntPtr handle, System.UInt32 length)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.ZeroMemory' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 status)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.FreeLibrary' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.IntPtr hModule, System.String lpProcName)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.GetProcAddress' has not been implemented!");
		}

		public static System.IntPtr LoadLibrary(System.String lpFileName)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.LoadLibrary' has not been implemented!");
		}

		public static System.Boolean CertControlStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.UInt32 dwFlags, System.UInt32 dwCtrlType, System.IntPtr pvCtrlPara)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertControlStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertCreateCertificateContext(System.UInt32 dwCertEncodingType, System.Security.Cryptography.SafeLocalAllocHandle pbCertEncoded, System.UInt32 cbCertEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertCreateCertificateContext' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertDuplicateCertificateContext(System.Security.Cryptography.SafeCertContextHandle pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertDuplicateCertificateContext' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertChainHandle CertDuplicateCertificateChain(System.IntPtr pChainContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertDuplicateCertificateChain' has not been implemented!");
		}

		public static System.Boolean CertFreeCertificateContext(System.IntPtr pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertFreeCertificateContext' has not been implemented!");
		}

		public static System.Boolean CertGetIntendedKeyUsage(System.UInt32 dwCertEncodingType, System.IntPtr pCertInfo, System.IntPtr pbKeyUsage, System.UInt32 cbKeyUsage)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetIntendedKeyUsage' has not been implemented!");
		}

		public static System.UInt32 CertGetPublicKeyLength(System.UInt32 dwCertEncodingType, System.IntPtr pPublicKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetPublicKeyLength' has not been implemented!");
		}

		public static System.Boolean CertGetValidUsages(System.UInt32 cCerts, System.IntPtr rghCerts, System.IntPtr cNumOIDs, System.Security.Cryptography.SafeLocalAllocHandle rghOIDs, System.IntPtr pcbOIDs)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertGetValidUsages' has not been implemented!");
		}

		public static System.Boolean CertSerializeCertificateStoreElement(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle pbElement, System.IntPtr pcbElement)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertSerializeCertificateStoreElement' has not been implemented!");
		}

		public static System.Boolean CertStrToNameW(System.UInt32 dwCertEncodingType, System.String pszX500, System.UInt32 dwStrType, System.IntPtr pvReserved, System.IntPtr pbEncoded, System.UInt32* pcbEncoded, System.IntPtr ppszError)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertStrToNameW' has not been implemented!");
		}

		public static System.Int32 CertVerifyTimeValidity(System.Runtime.InteropServices.ComTypes.FILETIME* pTimeToVerify, System.IntPtr pCertInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CertVerifyTimeValidity' has not been implemented!");
		}

		public static System.Boolean CryptAcquireCertificatePrivateKey(System.Security.Cryptography.SafeCertContextHandle pCert, System.UInt32 dwFlags, System.IntPtr pvReserved, System.Security.Cryptography.SafeCryptProvHandle* phCryptProv, System.UInt32* pdwKeySpec, System.Boolean* pfCallerFreeProv)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptAcquireCertificatePrivateKey' has not been implemented!");
		}

		public static System.Boolean CryptDecodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.IntPtr pbEncoded, System.UInt32 cbEncoded, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle pvStructInfo, System.IntPtr pcbStructInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptDecodeObject' has not been implemented!");
		}

		public static System.Boolean CryptDecodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.Byte[] pbEncoded, System.UInt32 cbEncoded, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle pvStructInfo, System.IntPtr pcbStructInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptDecodeObject' has not been implemented!");
		}

		public static System.Boolean CryptEncodeObject(System.UInt32 dwCertEncodingType, System.IntPtr lpszStructType, System.IntPtr pvStructInfo, System.Security.Cryptography.SafeLocalAllocHandle pbEncoded, System.IntPtr pcbEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptEncodeObject' has not been implemented!");
		}

		public static System.Boolean CryptEncodeObject(System.UInt32 dwCertEncodingType, System.String lpszStructType, System.IntPtr pvStructInfo, System.Security.Cryptography.SafeLocalAllocHandle pbEncoded, System.IntPtr pcbEncoded)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptEncodeObject' has not been implemented!");
		}

		public static System.IntPtr CryptFindOIDInfo(System.UInt32 dwKeyType, System.IntPtr pvKey, System.Security.Cryptography.OidGroup dwGroupId)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptFindOIDInfo' has not been implemented!");
		}

		public static System.Boolean CryptFormatObject(System.UInt32 dwCertEncodingType, System.UInt32 dwFormatType, System.UInt32 dwFormatStrType, System.IntPtr pFormatStruct, System.String lpszStructType, System.Byte[] pbEncoded, System.UInt32 cbEncoded, System.Security.Cryptography.SafeLocalAllocHandle pbFormat, System.IntPtr pcbFormat)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptFormatObject' has not been implemented!");
		}

		public static System.Boolean CryptFormatObject(System.UInt32 dwCertEncodingType, System.UInt32 dwFormatType, System.UInt32 dwFormatStrType, System.IntPtr pFormatStruct, System.IntPtr lpszStructType, System.Byte[] pbEncoded, System.UInt32 cbEncoded, System.Security.Cryptography.SafeLocalAllocHandle pbFormat, System.IntPtr pcbFormat)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptFormatObject' has not been implemented!");
		}

		public static System.Boolean CryptGetProvParam(System.Security.Cryptography.SafeCryptProvHandle hProv, System.UInt32 dwParam, System.IntPtr pbData, System.IntPtr pdwDataLen, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptGetProvParam' has not been implemented!");
		}

		public static System.Boolean CryptHashCertificate(System.IntPtr hCryptProv, System.UInt32 Algid, System.UInt32 dwFlags, System.IntPtr pbEncoded, System.UInt32 cbEncoded, System.IntPtr pbComputedHash, System.IntPtr pcbComputedHash)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptHashCertificate' has not been implemented!");
		}

		public static System.Boolean CryptHashPublicKeyInfo(System.IntPtr hCryptProv, System.UInt32 Algid, System.UInt32 dwFlags, System.UInt32 dwCertEncodingType, System.IntPtr pInfo, System.IntPtr pbComputedHash, System.IntPtr pcbComputedHash)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptHashPublicKeyInfo' has not been implemented!");
		}

		public static System.Boolean CryptMsgGetParam(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwParamType, System.UInt32 dwIndex, System.IntPtr pvData, System.IntPtr pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgGetParam' has not been implemented!");
		}

		public static System.Boolean CryptMsgGetParam(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwParamType, System.UInt32 dwIndex, System.Security.Cryptography.SafeLocalAllocHandle pvData, System.IntPtr pcbData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgGetParam' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToDecode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr hCryptProv, System.IntPtr pRecipientInfo, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgOpenToDecode' has not been implemented!");
		}

		public static System.Boolean CryptMsgUpdate(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.Byte[] pbData, System.UInt32 cbData, System.Boolean fFinal)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgUpdate' has not been implemented!");
		}

		public static System.Boolean CryptMsgUpdate(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.IntPtr pbData, System.UInt32 cbData, System.Boolean fFinal)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgUpdate' has not been implemented!");
		}

		public static System.Boolean CryptMsgVerifyCountersignatureEncoded(System.IntPtr hCryptProv, System.UInt32 dwEncodingType, System.IntPtr pbSignerInfo, System.UInt32 cbSignerInfo, System.IntPtr pbSignerInfoCountersignature, System.UInt32 cbSignerInfoCountersignature, System.IntPtr pciCountersigner)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPISafe.CryptMsgVerifyCountersignatureEncoded' has not been implemented!");
		}
	}
}
