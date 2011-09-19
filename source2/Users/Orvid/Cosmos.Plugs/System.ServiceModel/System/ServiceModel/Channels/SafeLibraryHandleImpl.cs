namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Channels.SafeLibraryHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Channels_SafeLibraryHandleImpl
	{

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.SafeLibraryHandle.FreeLibrary' has not been implemented!");
		}
	}
}
