namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.SecurityManager), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_SecurityManagerImpl
	{

		public static System.Boolean _SetThreadSecurity(System.Boolean bThreadSecurity)
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityManager._SetThreadSecurity' has not been implemented!");
		}

		public static System.Boolean IsSameType(System.String strLeft, System.String strRight)
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityManager.IsSameType' has not been implemented!");
		}

		public static System.Void GetGrantedPermissions(System.Runtime.CompilerServices.ObjectHandleOnStack retGranted, System.Runtime.CompilerServices.ObjectHandleOnStack retDenied, System.Runtime.CompilerServices.StackCrawlMarkHandle stackMark)
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityManager.GetGrantedPermissions' has not been implemented!");
		}
	}
}
