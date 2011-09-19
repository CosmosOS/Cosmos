namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ServiceModel.Activation.Interop.SafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ServiceModel_Activation_Interop_SafeNativeMethodsImpl
	{

		public static System.Boolean OpenThreadTokenCritical(System.IntPtr ThreadHandle, System.Security.Principal.TokenAccessLevels DesiredAccess, System.Boolean OpenAsSelf, System.ServiceModel.Activation.Interop.SafeCloseHandleCritical* TokenHandle)
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.Interop.SafeNativeMethods.OpenThreadTokenCritical' has not been implemented!");
		}

		public static System.IntPtr GetCurrentThread()
		{
			throw new System.NotImplementedException("Method 'System.ServiceModel.Activation.Interop.SafeNativeMethods.GetCurrentThread' has not been implemented!");
		}
	}
}
