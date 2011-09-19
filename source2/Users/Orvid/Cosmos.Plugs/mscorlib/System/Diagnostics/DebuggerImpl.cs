namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Diagnostics.Debugger), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Diagnostics_DebuggerImpl
	{

		public static System.Boolean IsDebuggerAttached()
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.IsDebuggerAttached' has not been implemented!");
		}

		public static System.Boolean IsLogging()
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.IsLogging' has not been implemented!");
		}

		public static System.Void CustomNotification(System.Diagnostics.ICustomDebuggerNotification data)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.CustomNotification' has not been implemented!");
		}

		public static System.Void Log(System.Int32 level, System.String category, System.String message)
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.Log' has not been implemented!");
		}

		public static System.Void BreakInternal()
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.BreakInternal' has not been implemented!");
		}

		public static System.Boolean LaunchInternal()
		{
			throw new System.NotImplementedException("Method 'System.Diagnostics.Debugger.LaunchInternal' has not been implemented!");
		}
	}
}
