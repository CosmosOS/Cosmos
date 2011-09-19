namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Media.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Media_SoundPlayer+UnsafeNativeMethodsImpl
	{

		public static System.Boolean PlaySound(System.String soundName, System.IntPtr hmod, System.Int32 soundFlags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.PlaySound' has not been implemented!");
		}

		public static System.Boolean PlaySound(System.Byte[] soundName, System.IntPtr hmod, System.Int32 soundFlags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.PlaySound' has not been implemented!");
		}

		public static System.IntPtr mmioOpen(System.String fileName, System.IntPtr not_used, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.mmioOpen' has not been implemented!");
		}

		public static System.Int32 mmioAscend(System.IntPtr hMIO, System.Media.SoundPlayer+NativeMethods+MMCKINFO lpck, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.mmioAscend' has not been implemented!");
		}

		public static System.Int32 mmioDescend(System.IntPtr hMIO, System.Media.SoundPlayer+NativeMethods+MMCKINFO lpck, System.Media.SoundPlayer+NativeMethods+MMCKINFO lcpkParent, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.mmioDescend' has not been implemented!");
		}

		public static System.Int32 mmioRead(System.IntPtr hMIO, System.Byte[] wf, System.Int32 cch)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.mmioRead' has not been implemented!");
		}

		public static System.Int32 mmioClose(System.IntPtr hMIO, System.Int32 flags)
		{
			throw new System.NotImplementedException("Method 'System.Media.SoundPlayer+UnsafeNativeMethods.mmioClose' has not been implemented!");
		}
	}
}
