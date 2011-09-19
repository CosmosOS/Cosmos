namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.EnterpriseServices.Wow64Helper), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_EnterpriseServices_Wow64HelperImpl
	{

		public static System.UInt32 GetSystemWow64Directory(System.Char[] buffer, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Wow64Helper.GetSystemWow64Directory' has not been implemented!");
		}

		public static System.Boolean IsWow64Process(System.IntPtr hProcess, System.Boolean* bIsWow)
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Wow64Helper.IsWow64Process' has not been implemented!");
		}

		public static System.IntPtr GetCurrentProcess()
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Wow64Helper.GetCurrentProcess' has not been implemented!");
		}
	}
}
