namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Web.ClientServices.Providers.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Web_ClientServices_Providers_UnsafeNativeMethodsImpl
	{

		public static System.Int32 InternetSetCookieW(System.String uri, System.String cookieName, System.String cookieValue)
		{
			throw new System.NotImplementedException("Method 'System.Web.ClientServices.Providers.UnsafeNativeMethods.InternetSetCookieW' has not been implemented!");
		}

		public static System.Int32 InternetGetCookieW(System.String uri, System.String cookieName, System.Text.StringBuilder cookieValue, System.Int32* dwSize)
		{
			throw new System.NotImplementedException("Method 'System.Web.ClientServices.Providers.UnsafeNativeMethods.InternetGetCookieW' has not been implemented!");
		}
	}
}
