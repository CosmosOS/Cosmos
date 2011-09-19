namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Deployment.Application.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Deployment_Application_NativeMethodsImpl
	{

		public static System.Void GetSystemInfo(System.Deployment.Application.NativeMethods+SYSTEM_INFO* sysInfo)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetSystemInfo' has not been implemented!");
		}

		public static System.Void GetNativeSystemInfo(System.Deployment.Application.NativeMethods+SYSTEM_INFO* sysInfo)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetNativeSystemInfo' has not been implemented!");
		}

		public static System.Boolean VerifyVersionInfo(System.Deployment.Application.NativeMethods+OSVersionInfoEx osvi, System.UInt32 dwTypeMask, System.UInt64 dwConditionMask)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.VerifyVersionInfo' has not been implemented!");
		}

		public static System.UInt64 VerSetConditionMask(System.UInt64 ConditionMask, System.UInt32 TypeMask, System.Byte Condition)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.VerSetConditionMask' has not been implemented!");
		}

		public static System.IntPtr LoadLibraryEx(System.String lpModuleName, System.IntPtr hFile, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.LoadLibraryEx' has not been implemented!");
		}

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.IntPtr FindResource(System.IntPtr hModule, System.String lpName, System.String lpType)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.FindResource' has not been implemented!");
		}

		public static System.IntPtr LoadResource(System.IntPtr hModule, System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.LoadResource' has not been implemented!");
		}

		public static System.IntPtr LockResource(System.IntPtr hglobal)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.LockResource' has not been implemented!");
		}

		public static System.UInt32 SizeofResource(System.IntPtr hModule, System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.SizeofResource' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.Runtime.InteropServices.HandleRef handle)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Int32 GetShortPathName(System.String LongPath, System.Text.StringBuilder ShortPath, System.Int32 BufferSize)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetShortPathName' has not been implemented!");
		}

		public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(System.String lpFileName, System.UInt32 dwDesiredAccess, System.UInt32 dwShareMode, System.IntPtr lpSecurityAttributes, System.UInt32 dwCreationDisposition, System.UInt32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateFile' has not been implemented!");
		}

		public static System.Void CorLaunchApplication(System.UInt32 hostType, System.String applicationFullName, System.Int32 manifestPathsCount, System.String[] manifestPaths, System.Int32 activationDataCount, System.String[] activationData, System.Deployment.Application.NativeMethods+PROCESS_INFORMATION processInformation)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CorLaunchApplication' has not been implemented!");
		}

		public static System.Void CreateAssemblyCache(System.Deployment.Application.NativeMethods+IAssemblyCache* ppAsmCache, System.Int32 reserved)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateAssemblyCache' has not been implemented!");
		}

		public static System.Object GetAssemblyIdentityFromFile(System.String filePath, System.Guid* riid)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetAssemblyIdentityFromFile' has not been implemented!");
		}

		public static System.Void CreateAssemblyNameObject(System.Deployment.Application.NativeMethods+IAssemblyName* ppEnum, System.String szAssemblyName, System.UInt32 dwFlags, System.IntPtr pvReserved)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateAssemblyNameObject' has not been implemented!");
		}

		public static System.Void CreateAssemblyEnum(System.Deployment.Application.NativeMethods+IAssemblyEnum* ppEnum, System.Deployment.Application.NativeMethods+IApplicationContext pAppCtx, System.Deployment.Application.NativeMethods+IAssemblyName pName, System.UInt32 dwFlags, System.IntPtr pvReserved)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateAssemblyEnum' has not been implemented!");
		}

		public static System.IntPtr CreateActCtxW(System.Deployment.Application.NativeMethods+ACTCTXW actCtx)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateActCtxW' has not been implemented!");
		}

		public static System.Void ReleaseActCtx(System.IntPtr hActCtx)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.ReleaseActCtx' has not been implemented!");
		}

		public static System.IntPtr GetModuleHandle(System.String moduleName)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetModuleHandle' has not been implemented!");
		}

		public static System.Int32 GetModuleFileName(System.IntPtr module, System.Text.StringBuilder fileName, System.Int32 size)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetModuleFileName' has not been implemented!");
		}

		public static System.UInt32 GetCurrentThreadId()
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetCurrentThreadId' has not been implemented!");
		}

		public static System.Boolean CreateUrlCacheEntry(System.String urlName, System.Int32 expectedFileSize, System.String fileExtension, System.Text.StringBuilder fileName, System.Int32 dwReserved)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CreateUrlCacheEntry' has not been implemented!");
		}

		public static System.Boolean CommitUrlCacheEntry(System.String lpszUrlName, System.String lpszLocalFileName, System.Int64 ExpireTime, System.Int64 LastModifiedTime, System.UInt32 CacheEntryType, System.String lpHeaderInfo, System.Int32 dwHeaderSize, System.String lpszFileExtension, System.String lpszOriginalUrl)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CommitUrlCacheEntry' has not been implemented!");
		}

		public static System.Object nCLRCreateInstance(System.Guid clsid, System.Guid iid)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.nCLRCreateInstance' has not been implemented!");
		}

		public static System.Void GetRequestedRuntimeInfo(System.String pExe, System.String pwszVersion, System.String pConfigurationFile, System.UInt32 startupFlags, System.UInt32 runtimeInfoFlags, System.Text.StringBuilder pDirectory, System.UInt32 dwDirectory, System.UInt32* dwDirectoryLength, System.Text.StringBuilder pVersion, System.UInt32 cchBuffer, System.UInt32* dwLength)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetRequestedRuntimeInfo' has not been implemented!");
		}

		public static System.Boolean InternetGetCookieW(System.String url, System.String cookieName, System.Text.StringBuilder cookieData, System.UInt32* bytes)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.InternetGetCookieW' has not been implemented!");
		}

		public static System.Void SHChangeNotify(System.Int32 eventID, System.UInt32 flags, System.IntPtr item1, System.IntPtr item2)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.SHChangeNotify' has not been implemented!");
		}

		public static System.UInt32 SHCreateItemFromParsingName(System.String pszPath, System.IntPtr pbc, System.Guid riid, System.Object* ppv)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.SHCreateItemFromParsingName' has not been implemented!");
		}

		public static System.UInt32 CoCreateInstance(System.Guid* clsid, System.Object punkOuter, System.Int32 context, System.Guid* iid, System.Object* o)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.CoCreateInstance' has not been implemented!");
		}

		public static System.Void GetClrMetaHostPolicy(System.Guid* clsid, System.Guid* iid, System.Deployment.Application.NativeMethods+IClrMetaHostPolicy* ClrMetaHostPolicy)
		{
			throw new System.NotImplementedException("Method 'System.Deployment.Application.NativeMethods.GetClrMetaHostPolicy' has not been implemented!");
		}
	}
}
