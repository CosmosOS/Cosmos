namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IO.IsolatedStorage.IsolatedStorageFile), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IO_IsolatedStorage_IsolatedStorageFileImpl
	{

		public static System.UInt64 GetUsage(System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle handle)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.GetUsage' has not been implemented!");
		}

		public static System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle Open(System.String infoFile, System.String syncName)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.Open' has not been implemented!");
		}

		public static System.Void Reserve(System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle handle, System.UInt64 plQuota, System.UInt64 plReserve, System.Boolean fFree)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.Reserve' has not been implemented!");
		}

		public static System.Void GetRootDir(System.IO.IsolatedStorage.IsolatedStorageScope scope, System.Runtime.CompilerServices.StringHandleOnStack retRootDir)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.GetRootDir' has not been implemented!");
		}

		public static System.Boolean Lock(System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle handle, System.Boolean fLock)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.Lock' has not been implemented!");
		}

		public static System.Void CreateDirectoryWithDacl(System.String path)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.CreateDirectoryWithDacl' has not been implemented!");
		}

		public static System.Boolean GetQuota(System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle scope, System.Int64* quota)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.GetQuota' has not been implemented!");
		}

		public static System.Void SetQuota(System.IO.IsolatedStorage.SafeIsolatedStorageFileHandle scope, System.Int64 quota)
		{
			throw new System.NotImplementedException("Method 'System.IO.IsolatedStorage.IsolatedStorageFile.SetQuota' has not been implemented!");
		}
	}
}
