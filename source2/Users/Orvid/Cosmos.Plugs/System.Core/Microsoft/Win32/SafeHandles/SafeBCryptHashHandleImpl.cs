namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeBCryptHashHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeBCryptHashHandleImpl
	{

		public static System.Security.Cryptography.BCryptNative+ErrorCode BCryptDestroyHash(System.IntPtr hHash)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeBCryptHashHandle.BCryptDestroyHash' has not been implemented!");
		}
	}
}
