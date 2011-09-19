namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.SecurityRuntime), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_SecurityRuntimeImpl
	{

		public static System.Security.FrameSecurityDescriptor GetSecurityObjectForFrame(System.Threading.StackCrawlMark* stackMark, System.Boolean create)
		{
			throw new System.NotImplementedException("Method 'System.Security.SecurityRuntime.GetSecurityObjectForFrame' has not been implemented!");
		}
	}
}
