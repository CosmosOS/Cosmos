namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.RNGCryptoServiceProvider), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_RNGCryptoServiceProviderImpl
	{

		public static System.Void GetBytes(System.Security.Cryptography.SafeProvHandle hProv, System.Byte[] randomBytes, System.Int32 count)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.RNGCryptoServiceProvider.GetBytes' has not been implemented!");
		}

		public static System.Void GetNonZeroBytes(System.Security.Cryptography.SafeProvHandle hProv, System.Byte[] randomBytes, System.Int32 count)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.RNGCryptoServiceProvider.GetNonZeroBytes' has not been implemented!");
		}
	}
}
