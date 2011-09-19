namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Permissions.FileIOAccess), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Permissions_FileIOAccessImpl
	{

		public static System.Boolean IsLocalDrive(System.String path)
		{
			throw new System.NotImplementedException("Method 'System.Security.Permissions.FileIOAccess.IsLocalDrive' has not been implemented!");
		}
	}
}
