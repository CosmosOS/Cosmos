namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Win32_UnsafeNativeMethodsImpl
	{

		public static System.Boolean GetFileAttributesEx(System.String name, System.Int32 fileInfoLevel, Microsoft.Win32.UnsafeNativeMethods+WIN32_FILE_ATTRIBUTE_DATA* data)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetFileAttributesEx' has not been implemented!");
		}

		public static System.Int32 GetModuleFileName(System.Runtime.InteropServices.HandleRef hModule, System.Text.StringBuilder buffer, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.GetModuleFileName' has not been implemented!");
		}

		public static System.Boolean CryptProtectData(System.Configuration.DATA_BLOB* inputData, System.String description, System.Configuration.DATA_BLOB* entropy, System.IntPtr pReserved, System.Configuration.CRYPTPROTECT_PROMPTSTRUCT* promptStruct, System.UInt32 flags, System.Configuration.DATA_BLOB* outputData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CryptProtectData' has not been implemented!");
		}

		public static System.Boolean CryptUnprotectData(System.Configuration.DATA_BLOB* inputData, System.IntPtr description, System.Configuration.DATA_BLOB* entropy, System.IntPtr pReserved, System.Configuration.CRYPTPROTECT_PROMPTSTRUCT* promptStruct, System.UInt32 flags, System.Configuration.DATA_BLOB* outputData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CryptUnprotectData' has not been implemented!");
		}

		public static System.Int32 CryptAcquireContext(Microsoft.Win32.SafeCryptContextHandle* phProv, System.String pszContainer, System.String pszProvider, System.UInt32 dwProvType, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CryptAcquireContext' has not been implemented!");
		}

		public static System.Int32 CryptReleaseContext(Microsoft.Win32.SafeCryptContextHandle hProv, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.CryptReleaseContext' has not been implemented!");
		}

		public static System.IntPtr LocalFree(System.IntPtr buf)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.LocalFree' has not been implemented!");
		}

		public static System.Boolean MoveFileEx(System.String lpExistingFileName, System.String lpNewFileName, System.Int32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Win32.UnsafeNativeMethods.MoveFileEx' has not been implemented!");
		}
	}
}
