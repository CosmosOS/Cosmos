namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Web.Security.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Web_Security_NativeMethodsImpl
	{

		public static System.Int32 DsGetDcName(System.String computerName, System.String domainName, System.IntPtr domainGuid, System.String siteName, System.UInt32 flags, System.IntPtr* domainControllerInfo)
		{
			throw new System.NotImplementedException("Method 'System.Web.Security.NativeMethods.DsGetDcName' has not been implemented!");
		}

		public static System.Int32 NetApiBufferFree(System.IntPtr buffer)
		{
			throw new System.NotImplementedException("Method 'System.Web.Security.NativeMethods.NetApiBufferFree' has not been implemented!");
		}

		public static System.Int32 FormatMessageW(System.Int32 dwFlags, System.Int32 lpSource, System.Int32 dwMessageId, System.Int32 dwLanguageId, System.Text.StringBuilder lpBuffer, System.Int32 nSize, System.Int32 arguments)
		{
			throw new System.NotImplementedException("Method 'System.Web.Security.NativeMethods.FormatMessageW' has not been implemented!");
		}
	}
}
