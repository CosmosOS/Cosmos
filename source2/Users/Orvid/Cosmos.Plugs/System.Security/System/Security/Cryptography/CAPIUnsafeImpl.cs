namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.CAPIUnsafe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_CAPI+CAPIUnsafeImpl
	{

		public static System.Boolean CryptAcquireContext(System.Security.Cryptography.SafeCryptProvHandle* hCryptProv, System.String pszContainer, System.String pszProvider, System.UInt32 dwProvType, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptAcquireContext' has not been implemented!");
		}

		public static System.Boolean CertAddCertificateContextToStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwAddDisposition, System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertAddCertificateContextToStore' has not been implemented!");
		}

		public static System.Boolean CertAddCertificateLinkToStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwAddDisposition, System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertAddCertificateLinkToStore' has not been implemented!");
		}

		public static System.IntPtr CertEnumCertificatesInStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.IntPtr pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertEnumCertificatesInStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertFindCertificateInStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.UInt32 dwCertEncodingType, System.UInt32 dwFindFlags, System.UInt32 dwFindType, System.IntPtr pvFindPara, System.Security.Cryptography.SafeCertContextHandle pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertFindCertificateInStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertStoreHandle CertOpenStore(System.IntPtr lpszStoreProvider, System.UInt32 dwMsgAndCertEncodingType, System.IntPtr hCryptProv, System.UInt32 dwFlags, System.String pvPara)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertOpenStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertCreateSelfSignCertificate(System.Security.Cryptography.SafeCryptProvHandle hProv, System.IntPtr pSubjectIssuerBlob, System.UInt32 dwFlags, System.IntPtr pKeyProvInfo, System.IntPtr pSignatureAlgorithm, System.IntPtr pStartTime, System.IntPtr pEndTime, System.IntPtr pExtensions)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CertCreateSelfSignCertificate' has not been implemented!");
		}

		public static System.Boolean CryptMsgControl(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwFlags, System.UInt32 dwCtrlType, System.IntPtr pvCtrlPara)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptMsgControl' has not been implemented!");
		}

		public static System.Boolean CryptMsgCountersign(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwIndex, System.UInt32 cCountersigners, System.IntPtr rgCountersigners)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptMsgCountersign' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToEncode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr pvMsgEncodeInfo, System.IntPtr pszInnerContentObjID, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptMsgOpenToEncode' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToEncode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr pvMsgEncodeInfo, System.String pszInnerContentObjID, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptMsgOpenToEncode' has not been implemented!");
		}

		public static System.Boolean CryptProtectData(System.IntPtr pDataIn, System.String szDataDescr, System.IntPtr pOptionalEntropy, System.IntPtr pvReserved, System.IntPtr pPromptStruct, System.UInt32 dwFlags, System.IntPtr pDataBlob)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptProtectData' has not been implemented!");
		}

		public static System.Boolean CryptUnprotectData(System.IntPtr pDataIn, System.IntPtr ppszDataDescr, System.IntPtr pOptionalEntropy, System.IntPtr pvReserved, System.IntPtr pPromptStruct, System.UInt32 dwFlags, System.IntPtr pDataBlob)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptUnprotectData' has not been implemented!");
		}

		public static System.Int32 SystemFunction040(System.Byte[] pDataIn, System.UInt32 cbDataIn, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.SystemFunction040' has not been implemented!");
		}

		public static System.Int32 SystemFunction041(System.Byte[] pDataIn, System.UInt32 cbDataIn, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.SystemFunction041' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CryptUIDlgSelectCertificateW(System.Security.Cryptography.CAPI+CRYPTUI_SELECTCERTIFICATE_STRUCTW csc)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptUIDlgSelectCertificateW' has not been implemented!");
		}

		public static System.Boolean CryptUIDlgViewCertificateW(System.Security.Cryptography.CAPI+CRYPTUI_VIEWCERTIFICATE_STRUCTW ViewInfo, System.IntPtr pfPropertiesChanged)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPI+CAPIUnsafe.CryptUIDlgViewCertificateW' has not been implemented!");
		}
	}
}
