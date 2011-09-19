namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeLibraryHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeLibraryHandleImpl
	{

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeLibraryHandle.FreeLibrary' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeLibraryHandle LoadLibraryEx(System.String libFilename, System.IntPtr reserved, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeLibraryHandle.LoadLibraryEx' has not been implemented!");
		}
	}
}
