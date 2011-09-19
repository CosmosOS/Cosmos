namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Cryptography.SafeProvHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Cryptography_SafeProvHandleImpl
	{

		public static System.Void FreeCsp(System.IntPtr pProviderContext)
		{
			throw new System.NotImplementedException("Method 'System.Security.Cryptography.SafeProvHandle.FreeCsp' has not been implemented!");
		}
	}
}
