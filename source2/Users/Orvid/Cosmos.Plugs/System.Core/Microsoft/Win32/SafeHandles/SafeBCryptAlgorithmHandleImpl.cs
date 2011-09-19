namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeBCryptAlgorithmHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeBCryptAlgorithmHandleImpl
	{

		public static System.Security.Cryptography.BCryptNative+ErrorCode BCryptCloseAlgorithmProvider(System.IntPtr hAlgorithm, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeBCryptAlgorithmHandle.BCryptCloseAlgorithmProvider' has not been implemented!");
		}
	}
}
