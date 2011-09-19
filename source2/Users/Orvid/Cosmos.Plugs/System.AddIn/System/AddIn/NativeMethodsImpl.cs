namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.AddIn.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_AddIn_NativeMethodsImpl
	{

		public static System.Boolean IsWow64Process(System.IntPtr hProcess, System.Boolean* bIsWow)
		{
			throw new System.NotImplementedException("Method 'System.AddIn.NativeMethods.IsWow64Process' has not been implemented!");
		}
	}
}
