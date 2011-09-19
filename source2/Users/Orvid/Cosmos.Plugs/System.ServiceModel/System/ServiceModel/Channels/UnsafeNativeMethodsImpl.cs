namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Channels.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Channels_UnsafeNativeMethodsImpl
	{

		public static System.Int32 CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Int32 ConnectNamedPipe(System.ServiceModel.Channels.PipeHandle handle, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.ConnectNamedPipe' has not been implemented!");
		}

		public static System.ServiceModel.Channels.PipeHandle CreateFile(System.String lpFileName, System.Int32 dwDesiredAccess, System.Int32 dwShareMode, System.IntPtr lpSECURITY_ATTRIBUTES, System.Int32 dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.CreateFile' has not been implemented!");
		}

		public static System.ServiceModel.Channels.SafeFileMappingHandle CreateFileMapping(System.IntPtr fileHandle, System.ServiceModel.Channels.UnsafeNativeMethods+SECURITY_ATTRIBUTES securityAttributes, System.Int32 protect, System.Int32 sizeHigh, System.Int32 sizeLow, System.String name)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.CreateFileMapping' has not been implemented!");
		}

		public static System.ServiceModel.Channels.PipeHandle CreateNamedPipe(System.String name, System.Int32 openMode, System.Int32 pipeMode, System.Int32 maxInstances, System.Int32 outBufSize, System.Int32 inBufSize, System.Int32 timeout, System.ServiceModel.Channels.UnsafeNativeMethods+SECURITY_ATTRIBUTES securityAttributes)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.CreateNamedPipe' has not been implemented!");
		}

		public static System.Int32 DisconnectNamedPipe(System.ServiceModel.Channels.PipeHandle handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.DisconnectNamedPipe' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr arguments)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.FormatMessage' has not been implemented!");
		}

		public static System.Int32 GetOverlappedResult(System.ServiceModel.Channels.PipeHandle handle, System.Threading.NativeOverlapped* overlapped, System.Int32* bytesTransferred, System.Int32 wait)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetOverlappedResult' has not been implemented!");
		}

		public static System.Int32 GetOverlappedResult(System.IntPtr handle, System.Threading.NativeOverlapped* overlapped, System.Int32* bytesTransferred, System.Int32 wait)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetOverlappedResult' has not been implemented!");
		}

		public static System.ServiceModel.Channels.SafeFileMappingHandle OpenFileMapping(System.Int32 access, System.Boolean inheritHandle, System.String name)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.OpenFileMapping' has not been implemented!");
		}

		public static System.ServiceModel.Channels.SafeViewOfFileHandle MapViewOfFile(System.ServiceModel.Channels.SafeFileMappingHandle handle, System.Int32 dwDesiredAccess, System.Int32 dwFileOffsetHigh, System.Int32 dwFileOffsetLow, System.IntPtr dwNumberOfBytesToMap)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MapViewOfFile' has not been implemented!");
		}

		public static System.Int32 QueryPerformanceCounter(System.Int64* time)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.QueryPerformanceCounter' has not been implemented!");
		}

		public static System.Int32 ReadFile(System.IntPtr handle, System.Byte* bytes, System.Int32 numBytesToRead, System.IntPtr numBytesRead_mustBeZero, System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.ReadFile' has not been implemented!");
		}

		public static System.Int32 SetNamedPipeHandleState(System.ServiceModel.Channels.PipeHandle handle, System.Int32* mode, System.IntPtr collectionCount, System.IntPtr collectionDataTimeout)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.SetNamedPipeHandleState' has not been implemented!");
		}

		public static System.Int32 WriteFile(System.IntPtr handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.IntPtr numBytesWritten_mustBeZero, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.WriteFile' has not been implemented!");
		}

		public static System.Int32 UnmapViewOfFile(System.IntPtr lpBaseAddress)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.UnmapViewOfFile' has not been implemented!");
		}

		public static System.Int32 WSARecv(System.IntPtr handle, System.ServiceModel.Channels.UnsafeNativeMethods+WSABuffer* buffers, System.Int32 bufferCount, System.Int32* bytesTransferred, System.Int32* socketFlags, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr completionRoutine)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.WSARecv' has not been implemented!");
		}

		public static System.Boolean WSAGetOverlappedResult(System.IntPtr socketHandle, System.Threading.NativeOverlapped* overlapped, System.Int32* bytesTransferred, System.Boolean wait, System.UInt32* flags)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.WSAGetOverlappedResult' has not been implemented!");
		}

		public static System.Int32 SspiFreeAuthIdentity(System.IntPtr ppAuthIdentity)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.SspiFreeAuthIdentity' has not been implemented!");
		}

		public static System.UInt32 SspiExcludePackage(System.IntPtr AuthIdentity, System.String pszPackageName, System.IntPtr* ppNewAuthIdentity)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.SspiExcludePackage' has not been implemented!");
		}

		public static System.Boolean DuplicateHandle(System.IntPtr hSourceProcessHandle, System.ServiceModel.Channels.PipeHandle hSourceHandle, System.ServiceModel.Activation.SafeCloseHandle hTargetProcessHandle, System.IntPtr* lpTargetHandle, System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.Int32 dwOptions)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.DuplicateHandle' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.ServiceModel.Channels.SafeLibraryHandle lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr arguments)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.FormatMessage' has not been implemented!");
		}

		public static System.Boolean GetNamedPipeClientProcessId(System.ServiceModel.Channels.PipeHandle handle, System.Int32* id)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetNamedPipeClientProcessId' has not been implemented!");
		}

		public static System.Boolean GetNamedPipeServerProcessId(System.ServiceModel.Channels.PipeHandle handle, System.Int32* id)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetNamedPipeServerProcessId' has not been implemented!");
		}

		public static System.Boolean SetWaitableTimer(Microsoft.Win32.SafeHandles.SafeWaitHandle handle, System.Int64* dueTime, System.Int32 period, System.IntPtr mustBeZero, System.IntPtr mustBeZeroAlso, System.Boolean resume)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.SetWaitableTimer' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateWaitableTimer(System.IntPtr mustBeZero, System.Boolean manualReset, System.String timerName)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.CreateWaitableTimer' has not been implemented!");
		}

		public static System.Int32 MQOpenQueue(System.String formatName, System.Int32 access, System.Int32 shareMode, System.ServiceModel.Channels.MsmqQueueHandle* handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQOpenQueue' has not been implemented!");
		}

		public static System.Int32 MQBeginTransaction(System.EnterpriseServices.ITransaction* refTransaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQBeginTransaction' has not been implemented!");
		}

		public static System.Int32 MQCloseQueue(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQCloseQueue' has not been implemented!");
		}

		public static System.Int32 MQSendMessage(System.ServiceModel.Channels.MsmqQueueHandle handle, System.IntPtr properties, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQSendMessage' has not been implemented!");
		}

		public static System.Int32 MQSendMessage(System.ServiceModel.Channels.MsmqQueueHandle handle, System.IntPtr properties, System.Transactions.IDtcTransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQSendMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr cursorHandle, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.IntPtr handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr cursorHandle, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr cursorHandle, System.Transactions.IDtcTransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.IntPtr handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr cursorHandle, System.Transactions.IDtcTransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.ServiceModel.Channels.UnsafeNativeMethods+MQReceiveCallback receiveCallback, System.IntPtr cursorHandle, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessage(System.IntPtr handle, System.Int32 timeout, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr cursorHandle, System.EnterpriseServices.ITransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessage' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessageByLookupId(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int64 lookupId, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.Transactions.IDtcTransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessageByLookupId' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessageByLookupId(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int64 lookupId, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessageByLookupId' has not been implemented!");
		}

		public static System.Int32 MQReceiveMessageByLookupId(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int64 lookupId, System.Int32 action, System.IntPtr properties, System.Threading.NativeOverlapped* nativeOverlapped, System.IntPtr receiveCallback, System.EnterpriseServices.ITransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReceiveMessageByLookupId' has not been implemented!");
		}

		public static System.Int32 MQGetPrivateComputerInformation(System.String computerName, System.IntPtr properties)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQGetPrivateComputerInformation' has not been implemented!");
		}

		public static System.Int32 MQMarkMessageRejected(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int64 lookupId)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQMarkMessageRejected' has not been implemented!");
		}

		public static System.Int32 MQMoveMessage(System.ServiceModel.Channels.MsmqQueueHandle sourceQueueHandle, System.ServiceModel.Channels.MsmqQueueHandle destinationQueueHandle, System.Int64 lookupId, System.IntPtr transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQMoveMessage' has not been implemented!");
		}

		public static System.Int32 MQMoveMessage(System.ServiceModel.Channels.MsmqQueueHandle sourceQueueHandle, System.ServiceModel.Channels.MsmqQueueHandle destinationQueueHandle, System.Int64 lookupId, System.Transactions.IDtcTransaction transaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQMoveMessage' has not been implemented!");
		}

		public static System.Int32 MQGetOverlappedResult(System.Threading.NativeOverlapped* nativeOverlapped)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQGetOverlappedResult' has not been implemented!");
		}

		public static System.Int32 MQGetQueueProperties(System.String formatName, System.IntPtr properties)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQGetQueueProperties' has not been implemented!");
		}

		public static System.Int32 MQPathNameToFormatName(System.String pathName, System.Text.StringBuilder formatName, System.Int32* count)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQPathNameToFormatName' has not been implemented!");
		}

		public static System.Int32 MQReplaceTransaction(System.EnterpriseServices.ITransaction sourceTransaction, System.Transactions.IDtcTransaction destinationTransaction)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQReplaceTransaction' has not been implemented!");
		}

		public static System.Int32 MQMgmtGetInfo(System.String computerName, System.String objectName, System.IntPtr properties)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQMgmtGetInfo' has not been implemented!");
		}

		public static System.Void MQFreeMemory(System.IntPtr nativeBuffer)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.MQFreeMemory' has not been implemented!");
		}

		public static System.Int32 GetHandleInformation(System.ServiceModel.Channels.MsmqQueueHandle handle, System.Int32* flags)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetHandleInformation' has not been implemented!");
		}

		public static System.Boolean GlobalMemoryStatusEx(System.ServiceModel.Channels.UnsafeNativeMethods+MEMORYSTATUSEX* lpBuffer)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GlobalMemoryStatusEx' has not been implemented!");
		}

		public static System.IntPtr VirtualAlloc(System.IntPtr lpAddress, System.UIntPtr dwSize, System.UInt32 flAllocationType, System.UInt32 flProtect)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.VirtualAlloc' has not been implemented!");
		}

		public static System.Boolean VirtualFree(System.IntPtr lpAddress, System.UIntPtr dwSize, System.UInt32 dwFreeType)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.VirtualFree' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.ServiceModel.Channels.SafeLibraryHandle hModule, System.String lpProcName)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetProcAddress' has not been implemented!");
		}

		public static System.ServiceModel.Channels.SafeLibraryHandle LoadLibrary(System.String libFilename)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.LoadLibrary' has not been implemented!");
		}

		public static System.Int32 BCryptGetFipsAlgorithmMode(System.Boolean* pfEnabled)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.BCryptGetFipsAlgorithmMode' has not been implemented!");
		}

		public static System.Boolean GetComputerNameEx(System.ServiceModel.Channels.ComputerNameFormat nameType, System.Text.StringBuilder lpBuffer, System.Int32* size)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Channels.UnsafeNativeMethods.GetComputerNameEx' has not been implemented!");
		}
	}
}
