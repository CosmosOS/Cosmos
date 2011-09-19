namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IdentityModel.SafeFreeContextBuffer), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IdentityModel_SafeFreeContextBufferImpl
	{

		public static System.Int32 QueryContextAttributesW(System.IdentityModel.SSPIHandle* contextHandle, System.IdentityModel.ContextAttribute attribute, System.Void* buffer)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeContextBuffer.QueryContextAttributesW' has not been implemented!");
		}

		public static System.Int32 EnumerateSecurityPackagesW(System.Int32* pkgnum, System.IdentityModel.SafeFreeContextBuffer* handle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeContextBuffer.EnumerateSecurityPackagesW' has not been implemented!");
		}

		public static System.Int32 FreeContextBuffer(System.IntPtr contextBuffer)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeContextBuffer.FreeContextBuffer' has not been implemented!");
		}
	}
}
