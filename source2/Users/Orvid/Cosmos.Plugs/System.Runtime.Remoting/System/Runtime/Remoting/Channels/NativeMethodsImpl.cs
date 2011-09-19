namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.Remoting.Channels.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_Remoting_Channels_NativeMethodsImpl
	{

		public static System.Boolean IsValidSid(System.IntPtr sidPointer)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.NativeMethods.IsValidSid' has not been implemented!");
		}

		public static System.IntPtr GetSidIdentifierAuthority(System.IntPtr sidPointer)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.NativeMethods.GetSidIdentifierAuthority' has not been implemented!");
		}

		public static System.IntPtr GetSidSubAuthorityCount(System.IntPtr sidPointer)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.NativeMethods.GetSidSubAuthorityCount' has not been implemented!");
		}

		public static System.IntPtr GetSidSubAuthority(System.IntPtr sidPointer, System.Int32 count)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.NativeMethods.GetSidSubAuthority' has not been implemented!");
		}

		public static System.Boolean GetTokenInformation(System.IntPtr tokenHandle, System.Int32 tokenInformationClass, System.IntPtr sidAndAttributesPointer, System.Int32 tokenInformationLength, System.Int32* returnLength)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.Remoting.Channels.NativeMethods.GetTokenInformation' has not been implemented!");
		}
	}
}
