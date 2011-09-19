namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Design.Shell32), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Design_UnsafeNativeMethods+Shell32Impl
	{

		public static System.Int32 SHGetSpecialFolderLocation(System.IntPtr hwnd, System.Int32 csidl, System.IntPtr* ppidl)
		{
			throw new System.NotImplementedException("Method 'System.Design.UnsafeNativeMethods+Shell32.SHGetSpecialFolderLocation' has not been implemented!");
		}

		public static System.Boolean SHGetPathFromIDList(System.IntPtr pidl, System.IntPtr pszPath)
		{
			throw new System.NotImplementedException("Method 'System.Design.UnsafeNativeMethods+Shell32.SHGetPathFromIDList' has not been implemented!");
		}

		public static System.IntPtr SHBrowseForFolder(System.Design.UnsafeNativeMethods+BROWSEINFO lpbi)
		{
			throw new System.NotImplementedException("Method 'System.Design.UnsafeNativeMethods+Shell32.SHBrowseForFolder' has not been implemented!");
		}

		public static System.Int32 SHGetMalloc(System.Design.UnsafeNativeMethods+IMalloc[] ppMalloc)
		{
			throw new System.NotImplementedException("Method 'System.Design.UnsafeNativeMethods+Shell32.SHGetMalloc' has not been implemented!");
		}
	}
}
