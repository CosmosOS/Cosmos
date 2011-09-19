namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.Monitor), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_MonitorImpl
	{

		public static System.Void Enter(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.Enter' has not been implemented!");
		}

		public static System.Void Exit(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.Exit' has not been implemented!");
		}

		public static System.Void ReliableEnter(System.Object obj, System.Boolean* lockTaken)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.ReliableEnter' has not been implemented!");
		}

		public static System.Void ReliableEnterTimeout(System.Object obj, System.Int32 timeout, System.Boolean* lockTaken)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.ReliableEnterTimeout' has not been implemented!");
		}

		public static System.Boolean ObjWait(System.Boolean exitContext, System.Int32 millisecondsTimeout, System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.ObjWait' has not been implemented!");
		}

		public static System.Void ObjPulse(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.ObjPulse' has not been implemented!");
		}

		public static System.Void ObjPulseAll(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.Threading.Monitor.ObjPulseAll' has not been implemented!");
		}
	}
}
