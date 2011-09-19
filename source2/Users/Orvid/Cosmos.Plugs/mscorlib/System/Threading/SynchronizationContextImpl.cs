namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.SynchronizationContext), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_SynchronizationContextImpl
	{

		public static System.Int32 WaitHelper(System.IntPtr[] waitHandles, System.Boolean waitAll, System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.Threading.SynchronizationContext.WaitHelper' has not been implemented!");
		}
	}
}
