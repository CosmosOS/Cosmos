namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.RSACryptoServiceProvider), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_RSACryptoServiceProviderImpl
	{

		public static System.Void DecryptKey(System.Security.Cryptography.SafeKeyHandle pKeyContext, System.Byte[] pbEncryptedKey, System.Int32 cbEncryptedKey, System.Boolean fOAEP, System.Runtime.CompilerServices.ObjectHandleOnStack ohRetDecryptedKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.RSACryptoServiceProvider.DecryptKey' has not been implemented!");
		}

		public static System.Void EncryptKey(System.Security.Cryptography.SafeKeyHandle pKeyContext, System.Byte[] pbKey, System.Int32 cbKey, System.Boolean fOAEP, System.Runtime.CompilerServices.ObjectHandleOnStack ohRetEncryptedKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.RSACryptoServiceProvider.EncryptKey' has not been implemented!");
		}
	}
}
