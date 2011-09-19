namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.SafeLocalAllocHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_SafeLocalAllocHandleImpl
	{

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.SafeLocalAllocHandle.LocalFree' has not been implemented!");
		}
	}
}
