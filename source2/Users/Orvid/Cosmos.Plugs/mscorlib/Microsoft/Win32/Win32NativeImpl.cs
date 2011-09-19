namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.Win32Native), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_Win32NativeImpl
	{

		public static System.Void GetSystemInfo(Microsoft.Win32.Win32Native+SYSTEM_INFO* lpSystemInfo)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetSystemInfo' has not been implemented!");
		}

		public static System.IntPtr LocalAlloc_NoSafeHandle(System.Int32 uFlags, System.IntPtr sizetdwBytes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LocalAlloc_NoSafeHandle' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeLocalAllocHandle LocalAlloc(System.Int32 uFlags, System.IntPtr sizetdwBytes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LocalAlloc' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LocalFree' has not been implemented!");
		}

		public static System.Void ZeroMemory(System.IntPtr handle, System.UInt32 length)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ZeroMemory' has not been implemented!");
		}

		public static System.UInt32 GetTempPath(System.Int32 bufferLen, System.Text.StringBuilder buffer)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetTempPath' has not been implemented!");
		}

		public static System.Int32 lstrlenA(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.lstrlenA' has not been implemented!");
		}

		public static System.Int32 lstrlenW(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.lstrlenW' has not been implemented!");
		}

		public static System.IntPtr SysAllocStringLen(System.String src, System.Int32 len)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysAllocStringLen' has not been implemented!");
		}

		public static System.IntPtr SysAllocStringByteLen(System.Byte[] str, System.UInt32 len)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysAllocStringByteLen' has not been implemented!");
		}

		public static System.UInt32 SysStringByteLen(System.IntPtr bstr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysStringByteLen' has not been implemented!");
		}

		public static System.Int32 SysStringLen(System.IntPtr bstr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysStringLen' has not been implemented!");
		}

		public static System.Void SysFreeString(System.IntPtr bstr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysFreeString' has not been implemented!");
		}

		public static System.Void CopyMemoryUni(System.IntPtr pdst, System.String psrc, System.IntPtr sizetcb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CopyMemoryUni' has not been implemented!");
		}

		public static System.Void CopyMemoryUni(System.Text.StringBuilder pdst, System.IntPtr psrc, System.IntPtr sizetcb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CopyMemoryUni' has not been implemented!");
		}

		public static System.Void CopyMemoryAnsi(System.Text.StringBuilder pdst, System.IntPtr psrc, System.IntPtr sizetcb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CopyMemoryAnsi' has not been implemented!");
		}

		public static System.Int32 GetACP()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetACP' has not been implemented!");
		}

		public static System.Boolean SetEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetEvent' has not been implemented!");
		}

		public static System.Boolean ResetEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ResetEvent' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateEvent(Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES lpSecurityAttributes, System.Boolean isManualReset, System.Boolean initialState, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateEvent' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateMutex(Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES lpSecurityAttributes, System.Boolean initialOwner, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateMutex' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle OpenMutex(System.Int32 desiredAccess, System.Boolean inheritHandle, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.OpenMutex' has not been implemented!");
		}

		public static System.Boolean ReleaseMutex(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReleaseMutex' has not been implemented!");
		}

		public static System.Int32 GetFullPathName(System.Char* path, System.Int32 numBufferChars, System.Char* buffer, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetFullPathName' has not been implemented!");
		}

		public static System.Int32 GetLongPathName(System.Char* path, System.Char* longPathBuffer, System.Int32 bufferLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetLongPathName' has not been implemented!");
		}

		public static System.Boolean UnmapViewOfFile(System.IntPtr lpBaseAddress)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.UnmapViewOfFile' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CloseHandle' has not been implemented!");
		}

		public static System.Int32 GetFileType(Microsoft.Win32.SafeHandles.SafeFileHandle handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetFileType' has not been implemented!");
		}

		public static System.Int32 ReadFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.Int32* numBytesRead, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReadFile' has not been implemented!");
		}

		public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.Int32* numBytesWritten, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.WriteFile' has not been implemented!");
		}

		public static System.Int32 GetFileSize(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, System.Int32* highSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetFileSize' has not been implemented!");
		}

		public static System.IntPtr GetStdHandle(System.Int32 nStdHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetStdHandle' has not been implemented!");
		}

		public static System.Boolean CopyFile(System.String src, System.String dst, System.Boolean failIfExists)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CopyFile' has not been implemented!");
		}

		public static System.Boolean CreateDirectory(System.String path, Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES lpSecurityAttributes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateDirectory' has not been implemented!");
		}

		public static System.Boolean DeleteFile(System.String path)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DeleteFile' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeFindHandle FindFirstFile(System.String fileName, Microsoft.Win32.Win32Native+WIN32_FIND_DATA data)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FindFirstFile' has not been implemented!");
		}

		public static System.Boolean FindNextFile(Microsoft.Win32.SafeHandles.SafeFindHandle hndFindFile, Microsoft.Win32.Win32Native+WIN32_FIND_DATA lpFindFileData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FindNextFile' has not been implemented!");
		}

		public static System.Boolean FindClose(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FindClose' has not been implemented!");
		}

		public static System.Int32 GetCurrentDirectory(System.Int32 nBufferLength, System.Text.StringBuilder lpBuffer)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetCurrentDirectory' has not been implemented!");
		}

		public static System.Boolean GetFileAttributesEx(System.String name, System.Int32 fileInfoLevel, Microsoft.Win32.Win32Native+WIN32_FILE_ATTRIBUTE_DATA* lpFileInformation)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetFileAttributesEx' has not been implemented!");
		}

		public static System.UInt32 GetTempFileName(System.String tmpPath, System.String prefix, System.UInt32 uniqueIdOrZero, System.Text.StringBuilder tmpFileName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetTempFileName' has not been implemented!");
		}

		public static System.Boolean SetCurrentDirectory(System.String path)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetCurrentDirectory' has not been implemented!");
		}

		public static System.Int32 SetErrorMode(System.Int32 newMode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetErrorMode' has not been implemented!");
		}

		public static System.Int32 WideCharToMultiByte(System.UInt32 cp, System.UInt32 flags, System.Char* pwzSource, System.Int32 cchSource, System.Byte* pbDestBuffer, System.Int32 cbDestBuffer, System.IntPtr null1, System.IntPtr null2)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.WideCharToMultiByte' has not been implemented!");
		}

		public static System.Boolean SetEnvironmentVariable(System.String lpName, System.String lpValue)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetEnvironmentVariable' has not been implemented!");
		}

		public static System.Int32 GetEnvironmentVariable(System.String lpName, System.Text.StringBuilder lpValue, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetEnvironmentVariable' has not been implemented!");
		}

		public static System.Char* GetEnvironmentStrings()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetEnvironmentStrings' has not been implemented!");
		}

		public static System.Boolean FreeEnvironmentStrings(System.Char* pStrings)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FreeEnvironmentStrings' has not been implemented!");
		}

		public static System.UInt32 GetCurrentProcessId()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetCurrentProcessId' has not been implemented!");
		}

		public static System.Boolean GetUserName(System.Text.StringBuilder lpBuffer, System.Int32* nSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetUserName' has not been implemented!");
		}

		public static System.Int32 GetComputerName(System.Text.StringBuilder nameBuffer, System.Int32* bufferSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetComputerName' has not been implemented!");
		}

		public static System.Int32 CoCreateGuid(System.Guid* guid)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CoCreateGuid' has not been implemented!");
		}

		public static System.IntPtr CoTaskMemAlloc(System.Int32 cb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CoTaskMemAlloc' has not been implemented!");
		}

		public static System.Void CoTaskMemFree(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CoTaskMemFree' has not been implemented!");
		}

		public static System.UInt32 GetConsoleOutputCP()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleOutputCP' has not been implemented!");
		}

		public static System.Int32 RegEnumKeyEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.Int32 dwIndex, System.Text.StringBuilder lpName, System.Int32* lpcbName, System.Int32[] lpReserved, System.Text.StringBuilder lpClass, System.Int32[] lpcbClass, System.Int64[] lpftLastWriteTime)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegEnumKeyEx' has not been implemented!");
		}

		public static System.Int32 RegEnumValue(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.Int32 dwIndex, System.Text.StringBuilder lpValueName, System.Int32* lpcbValueName, System.IntPtr lpReserved_MustBeZero, System.Int32[] lpType, System.Byte[] lpData, System.Int32[] lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegEnumValue' has not been implemented!");
		}

		public static System.Int32 RegOpenKeyEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpSubKey, System.Int32 ulOptions, System.Int32 samDesired, Microsoft.Win32.SafeHandles.SafeRegistryHandle* hkResult)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegOpenKeyEx' has not been implemented!");
		}

		public static System.Int32 RegQueryInfoKey(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.Text.StringBuilder lpClass, System.Int32[] lpcbClass, System.IntPtr lpReserved_MustBeZero, System.Int32* lpcSubKeys, System.Int32[] lpcbMaxSubKeyLen, System.Int32[] lpcbMaxClassLen, System.Int32* lpcValues, System.Int32[] lpcbMaxValueNameLen, System.Int32[] lpcbMaxValueLen, System.Int32[] lpcbSecurityDescriptor, System.Int32[] lpftLastWriteTime)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryInfoKey' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Byte[] lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Int32* lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Char[] lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegSetValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32 Reserved, Microsoft.Win32.RegistryValueKind dwType, System.Int32* lpData, System.Int32 cbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegSetValueEx' has not been implemented!");
		}

		public static System.Int32 RegSetValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32 Reserved, Microsoft.Win32.RegistryValueKind dwType, System.String lpData, System.Int32 cbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegSetValueEx' has not been implemented!");
		}

		public static System.Int32 SHGetFolderPath(System.IntPtr hwndOwner, System.Int32 nFolder, System.IntPtr hToken, System.Int32 dwFlags, System.Text.StringBuilder lpszPath)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SHGetFolderPath' has not been implemented!");
		}

		public static System.Int32 SystemFunction041(System.Security.SafeBSTRHandle pDataIn, System.UInt32 cbDataIn, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SystemFunction041' has not been implemented!");
		}

		public static System.Boolean CheckTokenMembership(Microsoft.Win32.SafeHandles.SafeTokenHandle TokenHandle, System.Byte[] SidToCheck, System.Boolean* IsMember)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CheckTokenMembership' has not been implemented!");
		}

		public static System.Int32 ConvertStringSidToSid(System.String stringSid, System.IntPtr* ByteArray)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ConvertStringSidToSid' has not been implemented!");
		}

		public static System.Int32 CreateWellKnownSid(System.Int32 sidType, System.Byte[] domainSid, System.Byte[] resultSid, System.UInt32* resultSidLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateWellKnownSid' has not been implemented!");
		}

		public static System.Boolean DuplicateTokenEx(Microsoft.Win32.SafeHandles.SafeTokenHandle hExistingToken, System.UInt32 dwDesiredAccess, System.IntPtr lpTokenAttributes, System.UInt32 ImpersonationLevel, System.UInt32 TokenType, Microsoft.Win32.SafeHandles.SafeTokenHandle* phNewToken)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DuplicateTokenEx' has not been implemented!");
		}

		public static System.IntPtr GetCurrentProcess()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetCurrentProcess' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(Microsoft.Win32.SafeHandles.SafeTokenHandle TokenHandle, System.UInt32 TokenInformationClass, Microsoft.Win32.SafeHandles.SafeLocalAllocHandle TokenInformation, System.UInt32 TokenInformationLength, System.UInt32* ReturnLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetTokenInformation' has not been implemented!");
		}

		public static System.UInt32 LsaOpenPolicy(System.String systemName, Microsoft.Win32.Win32Native+LSA_OBJECT_ATTRIBUTES* attributes, System.Int32 accessMask, Microsoft.Win32.SafeHandles.SafeLsaPolicyHandle* handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaOpenPolicy' has not been implemented!");
		}

		public static System.UInt32 LsaLookupSids(Microsoft.Win32.SafeHandles.SafeLsaPolicyHandle handle, System.Int32 count, System.IntPtr[] sids, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* referencedDomains, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* names)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaLookupSids' has not been implemented!");
		}

		public static System.Int32 LsaFreeMemory(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaFreeMemory' has not been implemented!");
		}

		public static System.Int32 LsaClose(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaClose' has not been implemented!");
		}

		public static System.Boolean OpenProcessToken(System.IntPtr ProcessToken, System.Security.Principal.TokenAccessLevels DesiredAccess, Microsoft.Win32.SafeHandles.SafeTokenHandle* TokenHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.OpenProcessToken' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(System.String lpFileName, System.Int32 dwDesiredAccess, System.IO.FileShare dwShareMode, Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateFile' has not been implemented!");
		}

		public static System.Int32 SetFilePointerWin32(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Int32 lo, System.Int32* hi, System.Int32 origin)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetFilePointerWin32' has not been implemented!");
		}

		public static System.Void SetLastError(System.Int32 errorCode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetLastError' has not been implemented!");
		}

		public static System.Int32 FormatMessage(System.Int32 dwFlags, System.IntPtr lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.IntPtr va_list_arguments)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FormatMessage' has not been implemented!");
		}

		public static System.Boolean GlobalMemoryStatusEx(Microsoft.Win32.Win32Native+MEMORYSTATUSEX buffer)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GlobalMemoryStatusEx' has not been implemented!");
		}

		public static System.IntPtr VirtualQuery(System.Void* address, Microsoft.Win32.Win32Native+MEMORY_BASIC_INFORMATION* buffer, System.IntPtr sizeOfBuffer)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.VirtualQuery' has not been implemented!");
		}

		public static System.Void* VirtualAlloc(System.Void* address, System.UIntPtr numBytes, System.Int32 commitOrReserve, System.Int32 pageProtectionMode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.VirtualAlloc' has not been implemented!");
		}

		public static System.Boolean VirtualFree(System.Void* address, System.UIntPtr numBytes, System.Int32 pageFreeMode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.VirtualFree' has not been implemented!");
		}

		public static System.IntPtr GetModuleHandle(System.String moduleName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetModuleHandle' has not been implemented!");
		}

		public static System.Boolean IsWow64Process(System.IntPtr hSourceProcessHandle, System.Boolean* isWow64)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.IsWow64Process' has not been implemented!");
		}

		public static System.Int32 SysStringLen(System.Security.SafeBSTRHandle bstr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SysStringLen' has not been implemented!");
		}

		public static System.Void CopyMemoryAnsi(System.IntPtr pdst, System.Text.StringBuilder psrc, System.IntPtr sizetcb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CopyMemoryAnsi' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle OpenEvent(System.Int32 desiredAccess, System.Boolean inheritHandle, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.OpenEvent' has not been implemented!");
		}

		public static System.Int32 GetFullPathName(System.String path, System.Int32 numBufferChars, System.Text.StringBuilder buffer, System.IntPtr mustBeZero)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetFullPathName' has not been implemented!");
		}

		public static System.Int32 GetLongPathName(System.String path, System.Text.StringBuilder longPathBuffer, System.Int32 bufferLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetLongPathName' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeFileMappingHandle CreateFileMapping(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, System.IntPtr lpAttributes, System.UInt32 fProtect, System.UInt32 dwMaximumSizeHigh, System.UInt32 dwMaximumSizeLow, System.String lpName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateFileMapping' has not been implemented!");
		}

		public static System.IntPtr MapViewOfFile(Microsoft.Win32.SafeHandles.SafeFileMappingHandle handle, System.UInt32 dwDesiredAccess, System.UInt32 dwFileOffsetHigh, System.UInt32 dwFileOffsetLow, System.UIntPtr dwNumerOfBytesToMap)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.MapViewOfFile' has not been implemented!");
		}

		public static System.Boolean SetEndOfFile(Microsoft.Win32.SafeHandles.SafeFileHandle hFile)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetEndOfFile' has not been implemented!");
		}

		public static System.Boolean FlushFileBuffers(Microsoft.Win32.SafeHandles.SafeFileHandle hFile)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FlushFileBuffers' has not been implemented!");
		}

		public static System.Int32 ReadFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.IntPtr numBytesRead_mustBeZero, System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReadFile' has not been implemented!");
		}

		public static System.Int32 WriteFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Byte* bytes, System.Int32 numBytesToWrite, System.IntPtr numBytesWritten_mustBeZero, System.Threading.NativeOverlapped* lpOverlapped)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.WriteFile' has not been implemented!");
		}

		public static System.Boolean GetDiskFreeSpaceEx(System.String drive, System.Int64* freeBytesForUser, System.Int64* totalBytes, System.Int64* freeBytes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetDiskFreeSpaceEx' has not been implemented!");
		}

		public static System.Int32 GetDriveType(System.String drive)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetDriveType' has not been implemented!");
		}

		public static System.Boolean GetVolumeInformation(System.String drive, System.Text.StringBuilder volumeName, System.Int32 volumeNameBufLen, System.Int32* volSerialNumber, System.Int32* maxFileNameLen, System.Int32* fileSystemFlags, System.Text.StringBuilder fileSystemName, System.Int32 fileSystemNameBufLen)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetVolumeInformation' has not been implemented!");
		}

		public static System.Boolean SetVolumeLabel(System.String driveLetter, System.String volumeName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetVolumeLabel' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceCounter(System.Int64* value)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.QueryPerformanceCounter' has not been implemented!");
		}

		public static System.Boolean QueryPerformanceFrequency(System.Int64* value)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.QueryPerformanceFrequency' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeWaitHandle CreateSemaphore(Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES lpSecurityAttributes, System.Int32 initialCount, System.Int32 maximumCount, System.String name)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateSemaphore' has not been implemented!");
		}

		public static System.Boolean ReleaseSemaphore(Microsoft.Win32.SafeHandles.SafeWaitHandle handle, System.Int32 releaseCount, System.Int32* previousCount)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReleaseSemaphore' has not been implemented!");
		}

		public static System.Int32 GetWindowsDirectory(System.Text.StringBuilder sb, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetWindowsDirectory' has not been implemented!");
		}

		public static System.Int32 GetSystemDirectory(System.Text.StringBuilder sb, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetSystemDirectory' has not been implemented!");
		}

		public static System.Boolean SetFileTime(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, Microsoft.Win32.Win32Native+FILE_TIME* creationTime, Microsoft.Win32.Win32Native+FILE_TIME* lastAccessTime, Microsoft.Win32.Win32Native+FILE_TIME* lastWriteTime)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetFileTime' has not been implemented!");
		}

		public static System.Boolean LockFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Int32 offsetLow, System.Int32 offsetHigh, System.Int32 countLow, System.Int32 countHigh)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LockFile' has not been implemented!");
		}

		public static System.Boolean UnlockFile(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.Int32 offsetLow, System.Int32 offsetHigh, System.Int32 countLow, System.Int32 countHigh)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.UnlockFile' has not been implemented!");
		}

		public static System.Boolean ReplaceFile(System.String replacedFileName, System.String replacementFileName, System.String backupFileName, System.Int32 dwReplaceFlags, System.IntPtr lpExclude, System.IntPtr lpReserved)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReplaceFile' has not been implemented!");
		}

		public static System.Boolean DecryptFile(System.String path, System.Int32 reservedMustBeZero)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DecryptFile' has not been implemented!");
		}

		public static System.Boolean EncryptFile(System.String path)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.EncryptFile' has not been implemented!");
		}

		public static System.Boolean SetFileAttributes(System.String name, System.Int32 attr)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetFileAttributes' has not been implemented!");
		}

		public static System.Int32 GetLogicalDrives()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetLogicalDrives' has not been implemented!");
		}

		public static System.Boolean MoveFile(System.String src, System.String dst)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.MoveFile' has not been implemented!");
		}

		public static System.Boolean DeleteVolumeMountPoint(System.String mountPoint)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DeleteVolumeMountPoint' has not been implemented!");
		}

		public static System.Boolean RemoveDirectory(System.String path)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RemoveDirectory' has not been implemented!");
		}

		public static System.Boolean SetConsoleCtrlHandler(Microsoft.Win32.Win32Native+ConsoleCtrlHandlerRoutine handler, System.Boolean addOrRemove)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleCtrlHandler' has not been implemented!");
		}

		public static System.IntPtr CoTaskMemRealloc(System.IntPtr pv, System.Int32 cb)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CoTaskMemRealloc' has not been implemented!");
		}

		public static System.Boolean SetConsoleMode(System.IntPtr hConsoleHandle, System.Int32 mode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleMode' has not been implemented!");
		}

		public static System.Boolean GetConsoleMode(System.IntPtr hConsoleHandle, System.Int32* mode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleMode' has not been implemented!");
		}

		public static System.Boolean Beep(System.Int32 frequency, System.Int32 duration)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.Beep' has not been implemented!");
		}

		public static System.Boolean GetConsoleScreenBufferInfo(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+CONSOLE_SCREEN_BUFFER_INFO* lpConsoleScreenBufferInfo)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleScreenBufferInfo' has not been implemented!");
		}

		public static System.Boolean SetConsoleScreenBufferSize(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+COORD size)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleScreenBufferSize' has not been implemented!");
		}

		public static Microsoft.Win32.Win32Native+COORD GetLargestConsoleWindowSize(System.IntPtr hConsoleOutput)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetLargestConsoleWindowSize' has not been implemented!");
		}

		public static System.Boolean FillConsoleOutputCharacter(System.IntPtr hConsoleOutput, System.Char character, System.Int32 nLength, Microsoft.Win32.Win32Native+COORD dwWriteCoord, System.Int32* pNumCharsWritten)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FillConsoleOutputCharacter' has not been implemented!");
		}

		public static System.Boolean FillConsoleOutputAttribute(System.IntPtr hConsoleOutput, System.Int16 wColorAttribute, System.Int32 numCells, Microsoft.Win32.Win32Native+COORD startCoord, System.Int32* pNumBytesWritten)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.FillConsoleOutputAttribute' has not been implemented!");
		}

		public static System.Boolean SetConsoleWindowInfo(System.IntPtr hConsoleOutput, System.Boolean absolute, Microsoft.Win32.Win32Native+SMALL_RECT* consoleWindow)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleWindowInfo' has not been implemented!");
		}

		public static System.Boolean SetConsoleTextAttribute(System.IntPtr hConsoleOutput, System.Int16 attributes)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleTextAttribute' has not been implemented!");
		}

		public static System.Boolean SetConsoleCursorPosition(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+COORD cursorPosition)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleCursorPosition' has not been implemented!");
		}

		public static System.Boolean GetConsoleCursorInfo(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+CONSOLE_CURSOR_INFO* cci)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleCursorInfo' has not been implemented!");
		}

		public static System.Boolean SetConsoleCursorInfo(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+CONSOLE_CURSOR_INFO* cci)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleCursorInfo' has not been implemented!");
		}

		public static System.Int32 GetConsoleTitle(System.Text.StringBuilder sb, System.Int32 capacity)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleTitle' has not been implemented!");
		}

		public static System.Boolean SetConsoleTitle(System.String title)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleTitle' has not been implemented!");
		}

		public static System.Boolean ReadConsoleInput(System.IntPtr hConsoleInput, Microsoft.Win32.Win32Native+InputRecord* buffer, System.Int32 numInputRecords_UseOne, System.Int32* numEventsRead)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReadConsoleInput' has not been implemented!");
		}

		public static System.Boolean PeekConsoleInput(System.IntPtr hConsoleInput, Microsoft.Win32.Win32Native+InputRecord* buffer, System.Int32 numInputRecords_UseOne, System.Int32* numEventsRead)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.PeekConsoleInput' has not been implemented!");
		}

		public static System.Boolean ReadConsoleOutput(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+CHAR_INFO* pBuffer, Microsoft.Win32.Win32Native+COORD bufferSize, Microsoft.Win32.Win32Native+COORD bufferCoord, Microsoft.Win32.Win32Native+SMALL_RECT* readRegion)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ReadConsoleOutput' has not been implemented!");
		}

		public static System.Boolean WriteConsoleOutput(System.IntPtr hConsoleOutput, Microsoft.Win32.Win32Native+CHAR_INFO* buffer, Microsoft.Win32.Win32Native+COORD bufferSize, Microsoft.Win32.Win32Native+COORD bufferCoord, Microsoft.Win32.Win32Native+SMALL_RECT* writeRegion)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.WriteConsoleOutput' has not been implemented!");
		}

		public static System.Int16 GetKeyState(System.Int32 virtualKeyCode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetKeyState' has not been implemented!");
		}

		public static System.UInt32 GetConsoleCP()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetConsoleCP' has not been implemented!");
		}

		public static System.Boolean SetConsoleCP(System.UInt32 codePage)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleCP' has not been implemented!");
		}

		public static System.Boolean SetConsoleOutputCP(System.UInt32 codePage)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetConsoleOutputCP' has not been implemented!");
		}

		public static System.Int32 RegConnectRegistry(System.String machineName, Microsoft.Win32.SafeHandles.SafeRegistryHandle key, Microsoft.Win32.SafeHandles.SafeRegistryHandle* result)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegConnectRegistry' has not been implemented!");
		}

		public static System.Int32 RegCreateKeyEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpSubKey, System.Int32 Reserved, System.String lpClass, System.Int32 dwOptions, System.Int32 samDesired, Microsoft.Win32.Win32Native+SECURITY_ATTRIBUTES lpSecurityAttributes, Microsoft.Win32.SafeHandles.SafeRegistryHandle* hkResult, System.Int32* lpdwDisposition)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegCreateKeyEx' has not been implemented!");
		}

		public static System.Int32 RegDeleteKey(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpSubKey)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegDeleteKey' has not been implemented!");
		}

		public static System.Int32 RegDeleteKeyEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpSubKey, System.Int32 samDesired, System.Int32 Reserved)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegDeleteKeyEx' has not been implemented!");
		}

		public static System.Int32 RegDeleteValue(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegDeleteValue' has not been implemented!");
		}

		public static System.Int32 RegFlushKey(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegFlushKey' has not been implemented!");
		}

		public static System.Int32 RegOpenKeyEx(System.IntPtr hKey, System.String lpSubKey, System.Int32 ulOptions, System.Int32 samDesired, Microsoft.Win32.SafeHandles.SafeRegistryHandle* hkResult)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegOpenKeyEx' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Int64* lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegQueryValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32[] lpReserved, System.Int32* lpType, System.Text.StringBuilder lpData, System.Int32* lpcbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegQueryValueEx' has not been implemented!");
		}

		public static System.Int32 RegSetValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32 Reserved, Microsoft.Win32.RegistryValueKind dwType, System.Byte[] lpData, System.Int32 cbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegSetValueEx' has not been implemented!");
		}

		public static System.Int32 RegSetValueEx(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, System.String lpValueName, System.Int32 Reserved, Microsoft.Win32.RegistryValueKind dwType, System.Int64* lpData, System.Int32 cbData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.RegSetValueEx' has not been implemented!");
		}

		public static System.Int32 ExpandEnvironmentStrings(System.String lpSrc, System.Text.StringBuilder lpDst, System.Int32 nSize)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ExpandEnvironmentStrings' has not been implemented!");
		}

		public static System.IntPtr LocalReAlloc(System.IntPtr handle, System.IntPtr sizetcbBytes, System.Int32 uFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LocalReAlloc' has not been implemented!");
		}

		public static System.Byte GetUserNameEx(System.Int32 format, System.Text.StringBuilder domainName, System.Int32* domainNameLen)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetUserNameEx' has not been implemented!");
		}

		public static System.Boolean LookupAccountName(System.String machineName, System.String accountName, System.Byte[] sid, System.Int32* sidLen, System.Text.StringBuilder domainName, System.Int32* domainNameLen, System.Int32* peUse)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LookupAccountName' has not been implemented!");
		}

		public static System.IntPtr GetProcessWindowStation()
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetProcessWindowStation' has not been implemented!");
		}

		public static System.Boolean GetUserObjectInformation(System.IntPtr hObj, System.Int32 nIndex, Microsoft.Win32.Win32Native+USEROBJECTFLAGS pvBuffer, System.Int32 nLength, System.Int32* lpnLengthNeeded)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetUserObjectInformation' has not been implemented!");
		}

		public static System.IntPtr SendMessageTimeout(System.IntPtr hWnd, System.Int32 Msg, System.IntPtr wParam, System.String lParam, System.UInt32 fuFlags, System.UInt32 uTimeout, System.IntPtr lpdwResult)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SendMessageTimeout' has not been implemented!");
		}

		public static System.Int32 SystemFunction040(System.Security.SafeBSTRHandle pDataIn, System.UInt32 cbDataIn, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SystemFunction040' has not been implemented!");
		}

		public static System.Int32 LsaNtStatusToWinError(System.Int32 status)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaNtStatusToWinError' has not been implemented!");
		}

		public static System.UInt32 BCryptGetFipsAlgorithmMode(System.Boolean* pfEnabled)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.BCryptGetFipsAlgorithmMode' has not been implemented!");
		}

		public static System.Boolean AdjustTokenPrivileges(Microsoft.Win32.SafeHandles.SafeTokenHandle TokenHandle, System.Boolean DisableAllPrivileges, Microsoft.Win32.Win32Native+TOKEN_PRIVILEGE* NewState, System.UInt32 BufferLength, Microsoft.Win32.Win32Native+TOKEN_PRIVILEGE* PreviousState, System.UInt32* ReturnLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.AdjustTokenPrivileges' has not been implemented!");
		}

		public static System.Boolean AllocateLocallyUniqueId(Microsoft.Win32.Win32Native+LUID* Luid)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.AllocateLocallyUniqueId' has not been implemented!");
		}

		public static System.Int32 ConvertSdToStringSd(System.Byte[] securityDescriptor, System.UInt32 requestedRevision, System.UInt32 securityInformation, System.IntPtr* resultString, System.UInt32* resultStringLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ConvertSdToStringSd' has not been implemented!");
		}

		public static System.Int32 ConvertStringSdToSd(System.String stringSd, System.UInt32 stringSdRevision, System.IntPtr* resultSd, System.UInt32* resultSdLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.ConvertStringSdToSd' has not been implemented!");
		}

		public static System.Boolean DuplicateHandle(System.IntPtr hSourceProcessHandle, System.IntPtr hSourceHandle, System.IntPtr hTargetProcessHandle, Microsoft.Win32.SafeHandles.SafeTokenHandle* lpTargetHandle, System.UInt32 dwDesiredAccess, System.Boolean bInheritHandle, System.UInt32 dwOptions)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DuplicateHandle' has not been implemented!");
		}

		public static System.Boolean DuplicateHandle(System.IntPtr hSourceProcessHandle, Microsoft.Win32.SafeHandles.SafeTokenHandle hSourceHandle, System.IntPtr hTargetProcessHandle, Microsoft.Win32.SafeHandles.SafeTokenHandle* lpTargetHandle, System.UInt32 dwDesiredAccess, System.Boolean bInheritHandle, System.UInt32 dwOptions)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DuplicateHandle' has not been implemented!");
		}

		public static System.Boolean DuplicateTokenEx(Microsoft.Win32.SafeHandles.SafeTokenHandle ExistingTokenHandle, System.Security.Principal.TokenAccessLevels DesiredAccess, System.IntPtr TokenAttributes, Microsoft.Win32.Win32Native+SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, System.Security.Principal.TokenType TokenType, Microsoft.Win32.SafeHandles.SafeTokenHandle* DuplicateTokenHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.DuplicateTokenEx' has not been implemented!");
		}

		public static System.Int32 IsEqualDomainSid(System.Byte[] sid1, System.Byte[] sid2, System.Boolean* result)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.IsEqualDomainSid' has not been implemented!");
		}

		public static System.UInt32 GetSecurityDescriptorLength(System.IntPtr byteArray)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetSecurityDescriptorLength' has not been implemented!");
		}

		public static System.UInt32 GetSecurityInfoByHandle(System.Runtime.InteropServices.SafeHandle handle, System.UInt32 objectType, System.UInt32 securityInformation, System.IntPtr* sidOwner, System.IntPtr* sidGroup, System.IntPtr* dacl, System.IntPtr* sacl, System.IntPtr* securityDescriptor)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetSecurityInfoByHandle' has not been implemented!");
		}

		public static System.UInt32 GetSecurityInfoByName(System.String name, System.UInt32 objectType, System.UInt32 securityInformation, System.IntPtr* sidOwner, System.IntPtr* sidGroup, System.IntPtr* dacl, System.IntPtr* sacl, System.IntPtr* securityDescriptor)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetSecurityInfoByName' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.IntPtr TokenHandle, System.UInt32 TokenInformationClass, Microsoft.Win32.SafeHandles.SafeLocalAllocHandle TokenInformation, System.UInt32 TokenInformationLength, System.UInt32* ReturnLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetTokenInformation' has not been implemented!");
		}

		public static System.Int32 GetWindowsAccountDomainSid(System.Byte[] sid, System.Byte[] resultSid, System.UInt32* resultSidLength)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetWindowsAccountDomainSid' has not been implemented!");
		}

		public static System.Int32 IsWellKnownSid(System.Byte[] sid, System.Int32 type)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.IsWellKnownSid' has not been implemented!");
		}

		public static System.Boolean LookupPrivilegeValue(System.String lpSystemName, System.String lpName, Microsoft.Win32.Win32Native+LUID* Luid)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LookupPrivilegeValue' has not been implemented!");
		}

		public static System.UInt32 LsaLookupNames(Microsoft.Win32.SafeHandles.SafeLsaPolicyHandle handle, System.Int32 count, Microsoft.Win32.Win32Native+UNICODE_STRING[] names, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* referencedDomains, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* sids)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaLookupNames' has not been implemented!");
		}

		public static System.UInt32 LsaLookupNames2(Microsoft.Win32.SafeHandles.SafeLsaPolicyHandle handle, System.Int32 flags, System.Int32 count, Microsoft.Win32.Win32Native+UNICODE_STRING[] names, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* referencedDomains, Microsoft.Win32.SafeHandles.SafeLsaMemoryHandle* sids)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaLookupNames2' has not been implemented!");
		}

		public static System.Int32 LsaConnectUntrusted(Microsoft.Win32.SafeHandles.SafeLsaLogonProcessHandle* LsaHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaConnectUntrusted' has not been implemented!");
		}

		public static System.Int32 LsaGetLogonSessionData(Microsoft.Win32.Win32Native+LUID* LogonId, Microsoft.Win32.SafeHandles.SafeLsaReturnBufferHandle* ppLogonSessionData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaGetLogonSessionData' has not been implemented!");
		}

		public static System.Int32 LsaLogonUser(Microsoft.Win32.SafeHandles.SafeLsaLogonProcessHandle LsaHandle, Microsoft.Win32.Win32Native+UNICODE_INTPTR_STRING* OriginName, System.UInt32 LogonType, System.UInt32 AuthenticationPackage, System.IntPtr AuthenticationInformation, System.UInt32 AuthenticationInformationLength, System.IntPtr LocalGroups, Microsoft.Win32.Win32Native+TOKEN_SOURCE* SourceContext, Microsoft.Win32.SafeHandles.SafeLsaReturnBufferHandle* ProfileBuffer, System.UInt32* ProfileBufferLength, Microsoft.Win32.Win32Native+LUID* LogonId, Microsoft.Win32.SafeHandles.SafeTokenHandle* Token, Microsoft.Win32.Win32Native+QUOTA_LIMITS* Quotas, System.Int32* SubStatus)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaLogonUser' has not been implemented!");
		}

		public static System.Int32 LsaLookupAuthenticationPackage(Microsoft.Win32.SafeHandles.SafeLsaLogonProcessHandle LsaHandle, Microsoft.Win32.Win32Native+UNICODE_INTPTR_STRING* PackageName, System.UInt32* AuthenticationPackage)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaLookupAuthenticationPackage' has not been implemented!");
		}

		public static System.Int32 LsaRegisterLogonProcess(Microsoft.Win32.Win32Native+UNICODE_INTPTR_STRING* LogonProcessName, Microsoft.Win32.SafeHandles.SafeLsaLogonProcessHandle* LsaHandle, System.IntPtr* SecurityMode)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaRegisterLogonProcess' has not been implemented!");
		}

		public static System.Int32 LsaDeregisterLogonProcess(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaDeregisterLogonProcess' has not been implemented!");
		}

		public static System.Int32 LsaFreeReturnBuffer(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.LsaFreeReturnBuffer' has not been implemented!");
		}

		public static System.UInt32 SetSecurityInfoByName(System.String name, System.UInt32 objectType, System.UInt32 securityInformation, System.Byte[] owner, System.Byte[] group, System.Byte[] dacl, System.Byte[] sacl)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetSecurityInfoByName' has not been implemented!");
		}

		public static System.UInt32 SetSecurityInfoByHandle(System.Runtime.InteropServices.SafeHandle handle, System.UInt32 objectType, System.UInt32 securityInformation, System.Byte[] owner, System.Byte[] group, System.Byte[] dacl, System.Byte[] sacl)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.SetSecurityInfoByHandle' has not been implemented!");
		}

		public static System.Int32 CreateAssemblyNameObject(Microsoft.Win32.IAssemblyName* ppEnum, System.String szAssemblyName, System.UInt32 dwFlags, System.IntPtr pvReserved)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateAssemblyNameObject' has not been implemented!");
		}

		public static System.Int32 CreateAssemblyEnum(Microsoft.Win32.IAssemblyEnum* ppEnum, Microsoft.Win32.IApplicationContext pAppCtx, Microsoft.Win32.IAssemblyName pName, System.UInt32 dwFlags, System.IntPtr pvReserved)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.CreateAssemblyEnum' has not been implemented!");
		}

		public static System.Int32 GetCalendarInfo(System.Int32 Locale, System.Int32 Calendar, System.Int32 CalType, System.Text.StringBuilder lpCalData, System.Int32 cchData, System.IntPtr lpValue)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetCalendarInfo' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.IntPtr hModule, System.String methodName)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.Win32Native.GetProcAddress' has not been implemented!");
		}
	}
}
