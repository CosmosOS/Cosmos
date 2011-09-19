namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Security.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Security_SecurityAuditHelper+NativeMethodsImpl
	{

		public static System.Boolean AuthzUnregisterSecurityEventSource(System.UInt32 dwFlags, System.IntPtr* providerHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.AuthzUnregisterSecurityEventSource' has not been implemented!");
		}

		public static System.Boolean CloseHandle(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.CloseHandle' has not been implemented!");
		}

		public static System.Boolean FreeLibrary(System.IntPtr hModule)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.FreeLibrary' has not been implemented!");
		}

		public static System.Boolean AuthzRegisterSecurityEventSource(System.UInt32 dwFlags, System.String szEventSourceName, System.ServiceModel.Security.SecurityAuditHelper+SafeSecurityAuditHandle* phEventProvider)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.AuthzRegisterSecurityEventSource' has not been implemented!");
		}

		public static System.Boolean AuthzReportSecurityEventFromParams(System.UInt32 dwFlags, System.ServiceModel.Security.SecurityAuditHelper+SafeSecurityAuditHandle providerHandle, System.UInt32 auditId, System.Byte[] securityIdentifier, System.ServiceModel.Security.SecurityAuditHelper+NativeMethods+AUDIT_PARAMS* auditParams)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.AuthzReportSecurityEventFromParams' has not been implemented!");
		}

		public static System.ServiceModel.Security.SecurityAuditHelper+SafeLoadLibraryHandle LoadLibraryExW(System.String lpwLibFileName, System.IntPtr hFile, System.UInt32 dwFlags)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.LoadLibraryExW' has not been implemented!");
		}

		public static System.IntPtr GetProcAddress(System.ServiceModel.Security.SecurityAuditHelper+SafeLoadLibraryHandle hModule, System.String lpProcName)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Security.SecurityAuditHelper+NativeMethods.GetProcAddress' has not been implemented!");
		}
	}
}
