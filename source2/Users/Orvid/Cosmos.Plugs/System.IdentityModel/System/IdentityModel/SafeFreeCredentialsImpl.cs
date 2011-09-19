namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IdentityModel.SafeFreeCredentials), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IdentityModel_SafeFreeCredentialsImpl
	{

		public static System.Int32 AcquireCredentialsHandleW(System.String principal, System.String moduleName, System.Int32 usage, System.Void* logonID, System.IdentityModel.AuthIdentityEx* authdata, System.Void* keyCallback, System.Void* keyArgument, System.IdentityModel.SSPIHandle* handlePtr, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeCredentials.AcquireCredentialsHandleW' has not been implemented!");
		}

		public static System.Int32 AcquireCredentialsHandleW(System.String principal, System.String moduleName, System.Int32 usage, System.Void* logonID, System.IntPtr zero, System.Void* keyCallback, System.Void* keyArgument, System.IdentityModel.SSPIHandle* handlePtr, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeCredentials.AcquireCredentialsHandleW' has not been implemented!");
		}

		public static System.Int32 AcquireCredentialsHandleW(System.String principal, System.String moduleName, System.Int32 usage, System.Void* logonID, System.IdentityModel.SecureCredential* authData, System.Void* keyCallback, System.Void* keyArgument, System.IdentityModel.SSPIHandle* handlePtr, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeCredentials.AcquireCredentialsHandleW' has not been implemented!");
		}

		public static System.Int32 FreeCredentialsHandle(System.IdentityModel.SSPIHandle* handlePtr)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeFreeCredentials.FreeCredentialsHandle' has not been implemented!");
		}
	}
}
