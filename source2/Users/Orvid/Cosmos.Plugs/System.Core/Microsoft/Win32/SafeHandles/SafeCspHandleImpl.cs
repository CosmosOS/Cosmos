namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeCspHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeCspHandleImpl
	{

		public static System.Boolean CryptContextAddRef(Microsoft.Win32.SafeHandles.SafeCspHandle hProv, System.IntPtr pdwReserved, System.Int32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeCspHandle.CryptContextAddRef' has not been implemented!");
		}

		public static System.Boolean CryptReleaseContext(System.IntPtr hProv, System.Int32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeCspHandle.CryptReleaseContext' has not been implemented!");
		}
	}
}
