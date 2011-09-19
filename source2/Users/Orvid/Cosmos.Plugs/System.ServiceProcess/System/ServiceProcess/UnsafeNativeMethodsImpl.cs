namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceProcess.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceProcess_UnsafeNativeMethodsImpl
	{

		public static System.Boolean ControlService(System.IntPtr serviceHandle, System.Int32 control, System.ServiceProcess.NativeMethods+SERVICE_STATUS* pStatus)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.ControlService' has not been implemented!");
		}

		public static System.Boolean QueryServiceStatus(System.IntPtr serviceHandle, System.ServiceProcess.NativeMethods+SERVICE_STATUS* pStatus)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.QueryServiceStatus' has not been implemented!");
		}

		public static System.Boolean EnumServicesStatus(System.IntPtr databaseHandle, System.Int32 serviceType, System.Int32 serviceState, System.IntPtr status, System.Int32 size, System.Int32* bytesNeeded, System.Int32* servicesReturned, System.Int32* resumeHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatus' has not been implemented!");
		}

		public static System.Boolean EnumServicesStatusEx(System.IntPtr databaseHandle, System.Int32 infolevel, System.Int32 serviceType, System.Int32 serviceState, System.IntPtr status, System.Int32 size, System.Int32* bytesNeeded, System.Int32* servicesReturned, System.Int32* resumeHandle, System.String group)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatusEx' has not been implemented!");
		}

		public static System.IntPtr OpenService(System.IntPtr databaseHandle, System.String serviceName, System.Int32 access)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.OpenService' has not been implemented!");
		}

		public static System.Boolean StartService(System.IntPtr serviceHandle, System.Int32 argNum, System.IntPtr argPtrs)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.StartService' has not been implemented!");
		}

		public static System.Boolean EnumDependentServices(System.IntPtr serviceHandle, System.Int32 serviceState, System.IntPtr bufferOfENUM_SERVICE_STATUS, System.Int32 bufSize, System.Int32* bytesNeeded, System.Int32* numEnumerated)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.EnumDependentServices' has not been implemented!");
		}

		public static System.Boolean QueryServiceConfig(System.IntPtr serviceHandle, System.IntPtr query_service_config_ptr, System.Int32 bufferSize, System.Int32* bytesNeeded)
		{
			throw new System.NotImplementedException("Method 'System.ServiceProcess.UnsafeNativeMethods.QueryServiceConfig' has not been implemented!");
		}
	}
}
