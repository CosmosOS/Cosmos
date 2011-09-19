namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.PasswordDeriveBytes), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_PasswordDeriveBytesImpl
	{

		public static System.Void DeriveKey(System.Security.Cryptography.SafeProvHandle hProv, System.Int32 algid, System.Int32 algidHash, System.Byte[] password, System.Int32 cbPassword, System.Int32 dwFlags, System.Byte[] IV, System.Int32 cbIV, System.Runtime.CompilerServices.ObjectHandleOnStack retKey)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.PasswordDeriveBytes.DeriveKey' has not been implemented!");
		}
	}
}
