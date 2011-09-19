namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.Remoting.Channels.Ipc.NativePipe), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_Remoting_Channels_Ipc_NativePipeImpl
	{

		public static System.Runtime.Remoting.Channels.Ipc.PipeHandle CreateNamedPipe(System.String lpName, System.UInt32 dwOpenMode, System.UInt32 dwPipeMode, System.UInt32 nMaxInstances, System.UInt32 nOutBufferSize, System.UInt32 nInBufferSize, System.UInt32 nDefaultTimeOut, System.Runtime.Remoting.Channels.Ipc.SECURITY_ATTRIBUTES pipeSecurityDescriptor)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.CreateNamedPipe' has not been implemented!");
		}

		public static System.Boolean ConnectNamedPipe(System.Runtime.Remoting.Channels.Ipc.PipeHandle hNamedPipe, System.Threading.Overlapped lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.ConnectNamedPipe' has not been implemented!");
		}

		public static System.Boolean ImpersonateNamedPipeClient(System.Runtime.Remoting.Channels.Ipc.PipeHandle hNamedPipe)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.ImpersonateNamedPipeClient' has not been implemented!");
		}

		public static System.Boolean RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.RevertToSelf' has not been implemented!");
		}

		public static System.Runtime.Remoting.Channels.Ipc.PipeHandle CreateFile(System.String lpFileName, System.UInt32 dwDesiredAccess, System.UInt32 dwShareMode, System.IntPtr attr, System.UInt32 dwCreationDisposition, System.UInt32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.CreateFile' has not been implemented!");
		}

		public static System.Boolean ReadFile(System.Runtime.Remoting.Channels.Ipc.PipeHandle hFile, System.Byte* lpBuffer, System.Int32 nNumberOfBytesToRead, System.Int32* lpNumberOfBytesRead, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.ReadFile' has not been implemented!");
		}

		public static System.Boolean ReadFile(System.Runtime.Remoting.Channels.Ipc.PipeHandle hFile, System.Byte* lpBuffer, System.Int32 nNumberOfBytesToRead, System.IntPtr numBytesRead_mustBeZero, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.ReadFile' has not been implemented!");
		}

		public static System.Boolean WriteFile(System.Runtime.Remoting.Channels.Ipc.PipeHandle hFile, System.Byte* lpBuffer, System.Int32 nNumberOfBytesToWrite, System.Int32* lpNumberOfBytesWritten, System.IntPtr lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.WriteFile' has not been implemented!");
		}

		public static System.Boolean WaitNamedPipe(System.String name, System.Int32 timeout)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.WaitNamedPipe' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr va_list_arguments)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.FormatMessage' has not been implemented!");
		}

		public static System.Int32 CloseHandle(System.IntPtr hObject)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.Ipc.NativePipe.CloseHandle' has not been implemented!");
		}
	}
}
