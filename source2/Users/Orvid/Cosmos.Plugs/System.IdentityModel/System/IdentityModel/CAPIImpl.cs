namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IdentityModel.CAPI), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IdentityModel_CAPIImpl
	{

		public static System.IdentityModel.SafeCertStoreHandle CertOpenStore(System.IntPtr lpszStoreProvider, System.UInt32 dwMsgAndCertEncodingType, System.IntPtr hCryptProv, System.UInt32 dwFlags, System.String pvPara)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertOpenStore' has not been implemented!");
		}

		public static System.Boolean CertCloseStore(System.IntPtr hCertStore, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertCloseStore' has not been implemented!");
		}

		public static System.Boolean CertFreeCertificateContext(System.IntPtr pCertContext)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertFreeCertificateContext' has not been implemented!");
		}

		public static System.IdentityModel.SafeCertContextHandle CertFindCertificateInStore(System.IdentityModel.SafeCertStoreHandle hCertStore, System.UInt32 dwCertEncodingType, System.UInt32 dwFindFlags, System.UInt32 dwFindType, System.IdentityModel.SafeHGlobalHandle pvFindPara, System.IdentityModel.SafeCertContextHandle pPrevCertContext)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertFindCertificateInStore' has not been implemented!");
		}

		public static System.Boolean CertAddCertificateLinkToStore(System.IdentityModel.SafeCertStoreHandle hCertStore, System.IntPtr pCertContext, System.UInt32 dwAddDisposition, System.IdentityModel.SafeCertContextHandle ppStoreContext)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertAddCertificateLinkToStore' has not been implemented!");
		}

		public static System.Boolean CertGetCertificateChain(System.IntPtr hChainEngine, System.IntPtr pCertContext, System.Runtime.InteropServices.ComTypes.FILETIME* pTime, System.IdentityModel.SafeCertStoreHandle hAdditionalStore, System.IdentityModel.CAPI+CERT_CHAIN_PARA* pChainPara, System.UInt32 dwFlags, System.IntPtr pvReserved, System.IdentityModel.SafeCertChainHandle* ppChainContext)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertGetCertificateChain' has not been implemented!");
		}

		public static System.Boolean CertVerifyCertificateChainPolicy(System.IntPtr pszPolicyOID, System.IdentityModel.SafeCertChainHandle pChainContext, System.IdentityModel.CAPI+CERT_CHAIN_POLICY_PARA* pPolicyPara, System.IdentityModel.CAPI+CERT_CHAIN_POLICY_STATUS* pPolicyStatus)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertVerifyCertificateChainPolicy' has not been implemented!");
		}

		public static System.Void CertFreeCertificateChain(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.CertFreeCertificateChain' has not been implemented!");
		}

		public static System.Int32 BCryptGetFipsAlgorithmMode(System.Boolean* pfEnabled)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.CAPI.BCryptGetFipsAlgorithmMode' has not been implemented!");
		}
	}
}
