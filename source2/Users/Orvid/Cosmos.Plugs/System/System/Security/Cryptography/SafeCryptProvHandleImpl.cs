namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.SafeCryptProvHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_SafeCryptProvHandleImpl
	{

		public static System.Boolean CryptReleaseContext(System.IntPtr hCryptProv, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.SafeCryptProvHandle.CryptReleaseContext' has not been implemented!");
		}
	}
}
