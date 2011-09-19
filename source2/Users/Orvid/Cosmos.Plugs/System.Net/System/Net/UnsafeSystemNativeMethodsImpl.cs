namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.UnsafeSystemNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_UnsafeSystemNativeMethodsImpl
	{

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeSystemNativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.Net.SafeLoadLibrary LoadLibraryExW(System.String lpwLibFileName, System.Void* hFile, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeSystemNativeMethods.LoadLibraryExW' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.Net.SafeLoadLibrary hModule, System.String entryPoint)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeSystemNativeMethods.GetProcAddress' has not been implemented!");
		}

		public static System.UInt32 FormatMessage(System.Net.FormatMessageFlags dwFlags, System.IntPtr lpSource, System.UInt32 dwMessageId, System.UInt32 dwLanguageId, System.IntPtr* lpBuffer, System.UInt32 nSize, System.IntPtr vaArguments)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeSystemNativeMethods.FormatMessage' has not been implemented!");
		}

		public static System.UInt32 LocalFree(System.IntPtr lpMem)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeSystemNativeMethods.LocalFree' has not been implemented!");
		}
	}
}
