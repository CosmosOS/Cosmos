namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.TimerBase), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_TimerBaseImpl
	{

		public static System.Void AddTimerNative(System.Threading.TimerBase aThis, System.Object state, System.UInt32 dueTime, System.UInt32 period, System.Threading.StackCrawlMark* stackMark)
		{
			throw new System.NotImplementedException("Method 'System.Threading.TimerBase.AddTimerNative' has not been implemented!");
		}

		public static System.Boolean ChangeTimerNative(System.Threading.TimerBase aThis, System.UInt32 dueTime, System.UInt32 period)
		{
			throw new System.NotImplementedException("Method 'System.Threading.TimerBase.ChangeTimerNative' has not been implemented!");
		}

		public static System.Boolean DeleteTimerNative(System.Threading.TimerBase aThis, System.Runtime.InteropServices.SafeHandle notifyObject)
		{
			throw new System.NotImplementedException("Method 'System.Threading.TimerBase.DeleteTimerNative' has not been implemented!");
		}
	}
}
