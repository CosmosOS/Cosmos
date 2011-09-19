namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceProcess.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceProcess_NativeMethodsImpl
	{

		public static System.IntPtr OpenService(System.IntPtr databaseHandle, System.String serviceName, System.Int32 access)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.OpenService' has not been implemented!");
		}

		public static System.IntPtr RegisterServiceCtrlHandler(System.String serviceName, System.Delegate callback)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.RegisterServiceCtrlHandler' has not been implemented!");
		}

		public static System.IntPtr RegisterServiceCtrlHandlerEx(System.String serviceName, System.Delegate callback, System.IntPtr userData)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.RegisterServiceCtrlHandlerEx' has not been implemented!");
		}

		public static System.Boolean SetServiceStatus(System.IntPtr serviceStatusHandle, System.ServiceProcess.NativeMethods+SERVICE_STATUS* status)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.SetServiceStatus' has not been implemented!");
		}

		public static System.Boolean StartServiceCtrlDispatcher(System.IntPtr entry)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.StartServiceCtrlDispatcher' has not been implemented!");
		}

		public static System.IntPtr CreateService(System.IntPtr databaseHandle, System.String serviceName, System.String displayName, System.Int32 access, System.Int32 serviceType, System.Int32 startType, System.Int32 errorControl, System.String binaryPath, System.String loadOrderGroup, System.IntPtr pTagId, System.String dependencies, System.String servicesStartName, System.String password)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.CreateService' has not been implemented!");
		}

		public static System.Boolean DeleteService(System.IntPtr serviceHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.DeleteService' has not been implemented!");
		}

		public static System.Int32 LsaOpenPolicy(System.ServiceProcess.NativeMethods+LSA_UNICODE_STRING systemName, System.IntPtr pointerObjectAttributes, System.Int32 desiredAccess, System.IntPtr* pointerPolicyHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.LsaOpenPolicy' has not been implemented!");
		}

		public static System.Int32 LsaAddAccountRights(System.IntPtr policyHandle, System.Byte[] accountSid, System.ServiceProcess.NativeMethods+LSA_UNICODE_STRING userRights, System.Int32 countOfRights)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.LsaAddAccountRights' has not been implemented!");
		}

		public static System.Int32 LsaRemoveAccountRights(System.IntPtr policyHandle, System.Byte[] accountSid, System.Boolean allRights, System.ServiceProcess.NativeMethods+LSA_UNICODE_STRING userRights, System.Int32 countOfRights)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.LsaRemoveAccountRights' has not been implemented!");
		}

		public static System.Int32 LsaEnumerateAccountRights(System.IntPtr policyHandle, System.Byte[] accountSid, System.IntPtr* pLsaUnicodeStringUserRights, System.Int32* RightsCount)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.LsaEnumerateAccountRights' has not been implemented!");
		}

		public static System.Boolean LookupAccountName(System.String systemName, System.String accountName, System.Byte[] sid, System.Int32[] sidLen, System.Char[] refDomainName, System.Int32[] domNameLen, System.Int32[] sidNameUse)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.LookupAccountName' has not been implemented!");
		}

		public static System.Boolean GetComputerName(System.Text.StringBuilder lpBuffer, System.Int32* nSize)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.GetComputerName' has not been implemented!");
		}

		public static System.Boolean ChangeServiceConfig2(System.IntPtr serviceHandle, System.UInt32 infoLevel, System.ServiceProcess.NativeMethods+SERVICE_DESCRIPTION* serviceDesc)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.ChangeServiceConfig2' has not been implemented!");
		}

		public static System.Boolean ChangeServiceConfig2(System.IntPtr serviceHandle, System.UInt32 infoLevel, System.ServiceProcess.NativeMethods+SERVICE_DELAYED_AUTOSTART_INFO* serviceDesc)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.NativeMethods.ChangeServiceConfig2' has not been implemented!");
		}
	}
}
