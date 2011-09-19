namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.SafeLibraryHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_SafeLibraryHandleImpl
	{

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.SafeLibraryHandle.FreeLibrary' has not been implemented!");
		}
	}
}
