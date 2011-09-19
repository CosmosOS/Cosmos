namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.SecurityContext), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_SecurityContextImpl
	{

		public static System.Security.WindowsImpersonationFlowMode GetImpersonationFlowMode()
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityContext.GetImpersonationFlowMode' has not been implemented!");
		}

		public static System.Boolean IsDefaultThreadSecurityInfo()
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityContext.IsDefaultThreadSecurityInfo' has not been implemented!");
		}
	}
}
