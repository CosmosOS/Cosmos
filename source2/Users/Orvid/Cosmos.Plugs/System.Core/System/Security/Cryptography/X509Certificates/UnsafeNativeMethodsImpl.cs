namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.X509Certificates.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_X509Certificates_X509Native+UnsafeNativeMethodsImpl
	{

		public static System.Int32 CertFreeAuthenticodeSignerInfo(System.Security.Cryptography.X509Certificates.X509Native+AXL_AUTHENTICODE_SIGNER_INFO* pSignerInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.X509Certificates.X509Native+UnsafeNativeMethods.CertFreeAuthenticodeSignerInfo' has not been implemented!");
		}

		public static System.Int32 CertFreeAuthenticodeTimestamperInfo(System.Security.Cryptography.X509Certificates.X509Native+AXL_AUTHENTICODE_TIMESTAMPER_INFO* pTimestamperInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.X509Certificates.X509Native+UnsafeNativeMethods.CertFreeAuthenticodeTimestamperInfo' has not been implemented!");
		}

		public static System.Int32 _AxlGetIssuerPublicKeyHash(System.IntPtr pCertContext, Microsoft.Win32.SafeHandles.SafeAxlBufferHandle* ppwszPublicKeyHash)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.X509Certificates.X509Native+UnsafeNativeMethods._AxlGetIssuerPublicKeyHash' has not been implemented!");
		}

		public static System.Int32 CertVerifyAuthenticodeLicense(System.Security.Cryptography.CapiNative+CRYPTOAPI_BLOB* pLicenseBlob, System.Security.Cryptography.X509Certificates.X509Native+AxlVerificationFlags dwFlags, System.Security.Cryptography.X509Certificates.X509Native+AXL_AUTHENTICODE_SIGNER_INFO* pSignerInfo, System.Security.Cryptography.X509Certificates.X509Native+AXL_AUTHENTICODE_TIMESTAMPER_INFO* pTimestamperInfo)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.X509Certificates.X509Native+UnsafeNativeMethods.CertVerifyAuthenticodeLicense' has not been implemented!");
		}
	}
}
