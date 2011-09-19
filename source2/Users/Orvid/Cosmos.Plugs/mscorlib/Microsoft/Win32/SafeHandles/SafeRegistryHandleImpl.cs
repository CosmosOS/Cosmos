namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeRegistryHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeRegistryHandleImpl
	{

		public static System.Int32 RegCloseKey(System.IntPtr hKey)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeRegistryHandle.RegCloseKey' has not been implemented!");
		}
	}
}
