namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Deployment.Internal.CodeSigning.Win32), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Deployment_Internal_CodeSigning_Win32Impl
	{

		public static System.IntPtr GetProcessHeap()
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.GetProcessHeap' has not been implemented!");
		}

		public static System.Boolean HeapFree(System.IntPtr hHeap, System.UInt32 dwFlags, System.IntPtr lpMem)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.HeapFree' has not been implemented!");
		}

		public static System.Int32 CertTimestampAuthenticodeLicense(System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pSignedLicenseBlob, System.String pwszTimestampURI, System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pTimestampSignatureBlob)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.CertTimestampAuthenticodeLicense' has not been implemented!");
		}

		public static System.Int32 CertVerifyAuthenticodeLicense(System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pLicenseBlob, System.UInt32 dwFlags, System.Deployment.Internal.CodeSigning.Win32+AXL_SIGNER_INFO* pSignerInfo, System.Deployment.Internal.CodeSigning.Win32+AXL_TIMESTAMPER_INFO* pTimestamperInfo)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.CertVerifyAuthenticodeLicense' has not been implemented!");
		}

		public static System.Int32 CertFreeAuthenticodeSignerInfo(System.Deployment.Internal.CodeSigning.Win32+AXL_SIGNER_INFO* pSignerInfo)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.CertFreeAuthenticodeSignerInfo' has not been implemented!");
		}

		public static System.Int32 CertFreeAuthenticodeTimestamperInfo(System.Deployment.Internal.CodeSigning.Win32+AXL_TIMESTAMPER_INFO* pTimestamperInfo)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32.CertFreeAuthenticodeTimestamperInfo' has not been implemented!");
		}

		public static System.Int32 _AxlGetIssuerPublicKeyHash(System.IntPtr pCertContext, System.IntPtr* ppwszPublicKeyHash)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32._AxlGetIssuerPublicKeyHash' has not been implemented!");
		}

		public static System.Int32 _AxlRSAKeyValueToPublicKeyToken(System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pModulusBlob, System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pExponentBlob, System.IntPtr* ppwszPublicKeyToken)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32._AxlRSAKeyValueToPublicKeyToken' has not been implemented!");
		}

		public static System.Int32 _AxlPublicKeyBlobToPublicKeyToken(System.Deployment.Internal.CodeSigning.Win32+CRYPT_DATA_BLOB* pCspPublicKeyBlob, System.IntPtr* ppwszPublicKeyToken)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Internal.CodeSigning.Win32._AxlPublicKeyBlobToPublicKeyToken' has not been implemented!");
		}
	}
}
