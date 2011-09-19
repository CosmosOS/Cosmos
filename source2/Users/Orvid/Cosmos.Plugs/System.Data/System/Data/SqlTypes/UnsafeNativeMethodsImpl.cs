namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Data.SqlTypes.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Data_SqlTypes_UnsafeNativeMethodsImpl
	{

		public static System.UInt32 NtCreateFile(Microsoft.Win32.SafeHandles.SafeFileHandle* fileHandle, System.Int32 desiredAccess, System.Data.SqlTypes.UnsafeNativeMethods+OBJECT_ATTRIBUTES* objectAttributes, System.Data.SqlTypes.UnsafeNativeMethods+IO_STATUS_BLOCK* ioStatusBlock, System.Int64* allocationSize, System.UInt32 fileAttributes, System.IO.FileShare shareAccess, System.UInt32 createDisposition, System.UInt32 createOptions, System.Runtime.InteropServices.SafeHandle eaBuffer, System.UInt32 eaLength)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.NtCreateFile' has not been implemented!");
		}

		public static System.Data.SqlTypes.UnsafeNativeMethods+FileType GetFileType(Microsoft.Win32.SafeHandles.SafeFileHandle hFile)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.GetFileType' has not been implemented!");
		}

		public static System.Int32 GetFullPathName(System.String path, System.Int32 numBufferChars, System.Text.StringBuilder buffer, System.IntPtr lpFilePartOrNull)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.GetFullPathName' has not been implemented!");
		}

		public static System.UInt32 SetErrorMode(System.UInt32 mode)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.SetErrorMode' has not been implemented!");
		}

		public static System.Boolean DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle, System.UInt32 ioControlCode, System.IntPtr inBuffer, System.UInt32 cbInBuffer, System.IntPtr outBuffer, System.UInt32 cbOutBuffer, System.UInt32* cbBytesReturned, System.IntPtr overlapped)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.DeviceIoControl' has not been implemented!");
		}

		public static System.UInt32 RtlNtStatusToDosError(System.UInt32 status)
		{
			throw new System.NotImplementedException("Method 'System.Data.SqlTypes.UnsafeNativeMethods.RtlNtStatusToDosError' has not been implemented!");
		}
	}
}
