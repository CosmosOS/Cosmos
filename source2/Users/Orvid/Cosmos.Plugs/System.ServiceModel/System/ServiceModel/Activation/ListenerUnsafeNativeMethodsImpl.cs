namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Activation.ListenerUnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Activation_ListenerUnsafeNativeMethodsImpl
	{

		public static System.Boolean LookupAccountName(System.String systemName, System.String accountName, System.Byte[] sid, System.UInt32* cbSid, System.Text.StringBuilder referencedDomainName, System.UInt32* cchReferencedDomainName, System.Int16* peUse)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.LookupAccountName' has not been implemented!");
		}

		public static System.Boolean GetKernelObjectSecurity(System.ServiceModel.Activation.SafeCloseHandle handle, System.Int32 securityInformation, System.Byte[] pSecurityDescriptor, System.Int32 nLength, System.Int32* lpnLengthNeeded)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.GetKernelObjectSecurity' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.ServiceModel.Activation.SafeCloseHandle tokenHandle, System.ServiceModel.Activation.ListenerUnsafeNativeMethods+TOKEN_INFORMATION_CLASS tokenInformationClass, System.Byte[] pTokenInformation, System.Int32 tokenInformationLength, System.Int32* returnLength)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.GetTokenInformation' has not been implemented!");
		}

		public static System.ServiceModel.Activation.SafeCloseHandle OpenProcess(System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.Int32 dwProcessId)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.OpenProcess' has not been implemented!");
		}

		public static System.Boolean OpenProcessToken(System.ServiceModel.Activation.SafeCloseHandle processHandle, System.Int32 desiredAccess, System.ServiceModel.Activation.SafeCloseHandle* tokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.OpenProcessToken' has not been implemented!");
		}

		public static System.Boolean SetKernelObjectSecurity(System.ServiceModel.Activation.SafeCloseHandle handle, System.Int32 securityInformation, System.Byte[] pSecurityDescriptor)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.SetKernelObjectSecurity' has not been implemented!");
		}

		public static System.Boolean IsDebuggerPresent()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.IsDebuggerPresent' has not been implemented!");
		}

		public static System.Void DebugBreak()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.DebugBreak' has not been implemented!");
		}

		public static System.Boolean AdjustTokenPrivileges(System.ServiceModel.Activation.SafeCloseHandle tokenHandle, System.Boolean disableAllPrivileges, System.ServiceModel.Activation.ListenerUnsafeNativeMethods+TOKEN_PRIVILEGES* newState, System.Int32 bufferLength, System.IntPtr previousState, System.IntPtr returnLength)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.AdjustTokenPrivileges' has not been implemented!");
		}

		public static System.Boolean LookupPrivilegeValue(System.IntPtr lpSystemName, System.String lpName, System.ServiceModel.ComIntegration.LUID* lpLuid)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.LookupPrivilegeValue' has not been implemented!");
		}

		public static System.Boolean CloseServiceHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.CloseServiceHandle' has not been implemented!");
		}

		public static System.IntPtr GetCurrentProcess()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.GetCurrentProcess' has not been implemented!");
		}

		public static System.ServiceModel.Activation.SafeServiceHandle OpenSCManager(System.String lpMachineName, System.String lpDatabaseName, System.Int32 dwDesiredAccess)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.OpenSCManager' has not been implemented!");
		}

		public static System.ServiceModel.Activation.SafeServiceHandle OpenService(System.ServiceModel.Activation.SafeServiceHandle hSCManager, System.String lpServiceName, System.Int32 dwDesiredAccess)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.OpenService' has not been implemented!");
		}

		public static System.Boolean QueryServiceConfig(System.ServiceModel.Activation.SafeServiceHandle hService, System.Byte[] pServiceConfig, System.Int32 cbBufSize, System.Int32* pcbBytesNeeded)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.QueryServiceConfig' has not been implemented!");
		}

		public static System.Boolean QueryServiceStatus(System.ServiceModel.Activation.SafeServiceHandle hService, System.ServiceModel.Activation.ListenerUnsafeNativeMethods+SERVICE_STATUS_PROCESS* status)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.QueryServiceStatus' has not been implemented!");
		}

		public static System.Boolean QueryServiceStatusEx(System.ServiceModel.Activation.SafeServiceHandle hService, System.Int32 InfoLevel, System.Byte[] pBuffer, System.Int32 cbBufSize, System.Int32* pcbBytesNeeded)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.QueryServiceStatusEx' has not been implemented!");
		}

		public static System.Boolean StartService(System.ServiceModel.Activation.SafeServiceHandle hSCManager, System.Int32 dwNumServiceArgs, System.String[] lpServiceArgVectors)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.ListenerUnsafeNativeMethods.StartService' has not been implemented!");
		}
	}
}
