namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Principal.Win32), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Principal_Win32Impl
	{

		public static System.Int32 OpenThreadToken(System.Security.Principal.TokenAccessLevels dwDesiredAccess, System.Security.Principal.WinSecurityContext OpenAs, Microsoft.Win32.SafeHandles.SafeTokenHandle* phThreadToken)
		{
			throw new System.NotImplementedException("Method 'System.Security.Principal.Win32.OpenThreadToken' has not been implemented!");
		}

		public static System.Int32 RevertToSelf()
		{
			throw new System.NotImplementedException("Method 'System.Security.Principal.Win32.RevertToSelf' has not been implemented!");
		}

		public static System.Int32 ImpersonateLoggedOnUser(Microsoft.Win32.SafeHandles.SafeTokenHandle hToken)
		{
			throw new System.NotImplementedException("Method 'System.Security.Principal.Win32.ImpersonateLoggedOnUser' has not been implemented!");
		}

		public static System.Int32 SetThreadToken(Microsoft.Win32.SafeHandles.SafeTokenHandle hToken)
		{
			throw new System.NotImplementedException("Method 'System.Security.Principal.Win32.SetThreadToken' has not been implemented!");
		}
	}
}
