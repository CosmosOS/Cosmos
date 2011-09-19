namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.CAPIUnsafe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_CAPIUnsafeImpl
	{

		public static System.Boolean CertAddCertificateLinkToStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwAddDisposition, System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertAddCertificateLinkToStore' has not been implemented!");
		}

		public static System.IntPtr CertEnumCertificatesInStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.IntPtr pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertEnumCertificatesInStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertStoreHandle CertOpenStore(System.IntPtr lpszStoreProvider, System.UInt32 dwMsgAndCertEncodingType, System.IntPtr hCryptProv, System.UInt32 dwFlags, System.String pvPara)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertOpenStore' has not been implemented!");
		}

		public static System.Boolean CryptAcquireContext(System.Security.Cryptography.SafeCryptProvHandle* hCryptProv, System.String pszContainer, System.String pszProvider, System.UInt32 dwProvType, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptAcquireContext' has not been implemented!");
		}

		public static System.Boolean CertAddCertificateContextToStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwAddDisposition, System.Security.Cryptography.SafeCertContextHandle ppStoreContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertAddCertificateContextToStore' has not been implemented!");
		}

		public static System.Boolean CertDeleteCertificateFromStore(System.Security.Cryptography.SafeCertContextHandle pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertDeleteCertificateFromStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertEnumCertificatesInStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.Security.Cryptography.SafeCertContextHandle pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertEnumCertificatesInStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertFindCertificateInStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.UInt32 dwCertEncodingType, System.UInt32 dwFindFlags, System.UInt32 dwFindType, System.IntPtr pvFindPara, System.Security.Cryptography.SafeCertContextHandle pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertFindCertificateInStore' has not been implemented!");
		}

		public static System.Boolean CertSaveStore(System.Security.Cryptography.SafeCertStoreHandle hCertStore, System.UInt32 dwMsgAndCertEncodingType, System.UInt32 dwSaveAs, System.UInt32 dwSaveTo, System.IntPtr pvSaveToPara, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertSaveStore' has not been implemented!");
		}

		public static System.Boolean CertSetCertificateContextProperty(System.IntPtr pCertContext, System.UInt32 dwPropId, System.UInt32 dwFlags, System.IntPtr pvData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertSetCertificateContextProperty' has not been implemented!");
		}

		public static System.Boolean CertSetCertificateContextProperty(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwPropId, System.UInt32 dwFlags, System.IntPtr pvData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertSetCertificateContextProperty' has not been implemented!");
		}

		public static System.Boolean CertSetCertificateContextProperty(System.Security.Cryptography.SafeCertContextHandle pCertContext, System.UInt32 dwPropId, System.UInt32 dwFlags, System.Security.Cryptography.SafeLocalAllocHandle safeLocalAllocHandle)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertSetCertificateContextProperty' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertContextHandle CertCreateSelfSignCertificate(System.Security.Cryptography.SafeCryptProvHandle hProv, System.IntPtr pSubjectIssuerBlob, System.UInt32 dwFlags, System.IntPtr pKeyProvInfo, System.IntPtr pSignatureAlgorithm, System.IntPtr pStartTime, System.IntPtr pEndTime, System.IntPtr pExtensions)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CertCreateSelfSignCertificate' has not been implemented!");
		}

		public static System.Boolean CryptMsgControl(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwFlags, System.UInt32 dwCtrlType, System.IntPtr pvCtrlPara)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptMsgControl' has not been implemented!");
		}

		public static System.Boolean CryptMsgCountersign(System.Security.Cryptography.SafeCryptMsgHandle hCryptMsg, System.UInt32 dwIndex, System.UInt32 cCountersigners, System.IntPtr rgCountersigners)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptMsgCountersign' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToEncode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr pvMsgEncodeInfo, System.IntPtr pszInnerContentObjID, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptMsgOpenToEncode' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCryptMsgHandle CryptMsgOpenToEncode(System.UInt32 dwMsgEncodingType, System.UInt32 dwFlags, System.UInt32 dwMsgType, System.IntPtr pvMsgEncodeInfo, System.String pszInnerContentObjID, System.IntPtr pStreamInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptMsgOpenToEncode' has not been implemented!");
		}

		public static System.Boolean CryptQueryObject(System.UInt32 dwObjectType, System.IntPtr pvObject, System.UInt32 dwExpectedContentTypeFlags, System.UInt32 dwExpectedFormatTypeFlags, System.UInt32 dwFlags, System.IntPtr pdwMsgAndCertEncodingType, System.IntPtr pdwContentType, System.IntPtr pdwFormatType, System.IntPtr phCertStore, System.IntPtr phMsg, System.IntPtr ppvContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptQueryObject' has not been implemented!");
		}

		public static System.Boolean CryptQueryObject(System.UInt32 dwObjectType, System.IntPtr pvObject, System.UInt32 dwExpectedContentTypeFlags, System.UInt32 dwExpectedFormatTypeFlags, System.UInt32 dwFlags, System.IntPtr pdwMsgAndCertEncodingType, System.IntPtr pdwContentType, System.IntPtr pdwFormatType, System.Security.Cryptography.SafeCertStoreHandle* phCertStore, System.IntPtr phMsg, System.IntPtr ppvContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptQueryObject' has not been implemented!");
		}

		public static System.Boolean CryptProtectData(System.IntPtr pDataIn, System.String szDataDescr, System.IntPtr pOptionalEntropy, System.IntPtr pvReserved, System.IntPtr pPromptStruct, System.UInt32 dwFlags, System.IntPtr pDataBlob)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptProtectData' has not been implemented!");
		}

		public static System.Boolean CryptUnprotectData(System.IntPtr pDataIn, System.IntPtr ppszDataDescr, System.IntPtr pOptionalEntropy, System.IntPtr pvReserved, System.IntPtr pPromptStruct, System.UInt32 dwFlags, System.IntPtr pDataBlob)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.CryptUnprotectData' has not been implemented!");
		}

		public static System.Boolean PFXExportCertStore(System.Security.Cryptography.SafeCertStoreHandle hStore, System.IntPtr pPFX, System.String szPassword, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.PFXExportCertStore' has not been implemented!");
		}

		public static System.Security.Cryptography.SafeCertStoreHandle PFXImportCertStore(System.IntPtr pPFX, System.String szPassword, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.CAPIUnsafe.PFXImportCertStore' has not been implemented!");
		}
	}
}
