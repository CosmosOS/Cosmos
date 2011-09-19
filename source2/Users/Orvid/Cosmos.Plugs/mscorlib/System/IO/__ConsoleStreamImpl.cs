namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IO.__ConsoleStream), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IO___ConsoleStreamImpl
	{

		public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.Int32* numBytesWritten, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'System.IO.__ConsoleStream.WriteFile' has not been implemented!");
		}

		public static System.Int32 ReadFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.Int32* numBytesRead, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'System.IO.__ConsoleStream.ReadFile' has not been implemented!");
		}

		public static System.Void WaitForAvailableConsoleInput(Microsoft.Win32.SafeHandles.SafeFileHandle file)
		{
			throw new System.NotImplementedException("Method 'System.IO.__ConsoleStream.WaitForAvailableConsoleInput' has not been implemented!");
		}
	}
}
