namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.EnterpriseServices.Internal.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_EnterpriseServices_Internal_NativeMethodsImpl
	{

		public static System.Boolean CloseHandle(System.IntPtr Handle)
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Internal.NativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Boolean OpenThreadToken(System.IntPtr ThreadHandle, System.UInt32 DesiredAccess, System.Boolean OpenAsSelf, System.EnterpriseServices.Internal.SafeUserTokenHandle* TokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Internal.NativeMethods.OpenThreadToken' has not been implemented!");
		}

		public static System.Boolean SetThreadToken(System.IntPtr Thread, System.EnterpriseServices.Internal.SafeUserTokenHandle Token)
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Internal.NativeMethods.SetThreadToken' has not been implemented!");
		}

		public static System.IntPtr GetCurrentThread()
		{
			throw new System.NotImplementedException("Method 'System.EnterpriseServices.Internal.NativeMethods.GetCurrentThread' has not been implemented!");
		}
	}
}
