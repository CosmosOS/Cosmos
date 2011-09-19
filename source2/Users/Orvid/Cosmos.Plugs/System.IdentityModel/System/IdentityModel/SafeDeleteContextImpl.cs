namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.IdentityModel.SafeDeleteContext), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_IdentityModel_SafeDeleteContextImpl
	{

		public static System.Int32 QuerySecurityContextToken(System.IdentityModel.SSPIHandle* phContext, System.IdentityModel.SafeCloseHandle* handle)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.QuerySecurityContextToken' has not been implemented!");
		}

		public static System.Int32 InitializeSecurityContextW(System.IdentityModel.SSPIHandle* credentialHandle, System.Void* inContextPtr, System.Byte* targetName, System.IdentityModel.SspiContextFlags inFlags, System.Int32 reservedI, System.IdentityModel.Endianness endianness, System.IdentityModel.SecurityBufferDescriptor inputBuffer, System.Int32 reservedII, System.IdentityModel.SSPIHandle* outContextPtr, System.IdentityModel.SecurityBufferDescriptor outputBuffer, System.IdentityModel.SspiContextFlags* attributes, System.Int64* timestamp)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.InitializeSecurityContextW' has not been implemented!");
		}

		public static System.Int32 AcceptSecurityContext(System.IdentityModel.SSPIHandle* credentialHandle, System.Void* inContextPtr, System.IdentityModel.SecurityBufferDescriptor inputBuffer, System.IdentityModel.SspiContextFlags inFlags, System.IdentityModel.Endianness endianness, System.IdentityModel.SSPIHandle* outContextPtr, System.IdentityModel.SecurityBufferDescriptor outputBuffer, System.IdentityModel.SspiContextFlags* attributes, System.Int64* timestamp)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.AcceptSecurityContext' has not been implemented!");
		}

		public static System.Int32 DeleteSecurityContext(System.IdentityModel.SSPIHandle* handlePtr)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.DeleteSecurityContext' has not been implemented!");
		}

		public static System.Int32 ImpersonateSecurityContext(System.IdentityModel.SSPIHandle* handlePtr)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.ImpersonateSecurityContext' has not been implemented!");
		}

		public static System.Int32 EncryptMessage(System.IdentityModel.SSPIHandle* contextHandle, System.UInt32 qualityOfProtection, System.IdentityModel.SecurityBufferDescriptor inputOutput, System.UInt32 sequenceNumber)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.EncryptMessage' has not been implemented!");
		}

		public static System.Int32 DecryptMessage(System.IdentityModel.SSPIHandle* contextHandle, System.IdentityModel.SecurityBufferDescriptor inputOutput, System.UInt32 sequenceNumber, System.UInt32* qualityOfProtection)
		{
			throw new System.NotImplementedException("Method 'System.IdentityModel.SafeDeleteContext.DecryptMessage' has not been implemented!");
		}
	}
}
