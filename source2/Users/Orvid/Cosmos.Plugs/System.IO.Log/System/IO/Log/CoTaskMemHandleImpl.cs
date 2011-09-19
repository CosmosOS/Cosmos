namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IO.Log.CoTaskMemHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IO_Log_CoTaskMemHandleImpl
	{

		public static System.Void CoTaskMemFree(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.IO.Log.CoTaskMemHandle.CoTaskMemFree' has not been implemented!");
		}
	}
}
