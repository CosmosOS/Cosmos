namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.SafeHandles.SafeFileMapViewHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_SafeHandles_SafeFileMapViewHandleImpl
	{

		public static System.Boolean UnmapViewOfFile(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeFileMapViewHandle.UnmapViewOfFile' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeFileMapViewHandle MapViewOfFile(Microsoft.Win32.SafeHandles.SafeFileMappingHandle hFileMappingObject, System.Int32 dwDesiredAccess, System.Int32 dwFileOffsetHigh, System.Int32 dwFileOffsetLow, System.UIntPtr dwNumberOfBytesToMap)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.SafeHandles.SafeFileMapViewHandle.MapViewOfFile' has not been implemented!");
		}
	}
}
