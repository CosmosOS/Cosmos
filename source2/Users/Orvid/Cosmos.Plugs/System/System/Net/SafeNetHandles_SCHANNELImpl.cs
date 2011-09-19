namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Net.SafeNetHandles_SCHANNEL), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Net_UnsafeNclNativeMethods+SafeNetHandles_SCHANNELImpl
	{

		public static System.Int32 FreeContextBuffer(System.IntPtr contextBuffer)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.FreeContextBuffer' has not been implemented!");
		}

		public static System.Int32 QueryContextAttributesA(System.Net.SSPIHandle* contextHandle, System.Net.ContextAttribute attribute, System.Void* buffer)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.QueryContextAttributesA' has not been implemented!");
		}

		public static System.Int32 InitializeSecurityContextA(System.Net.SSPIHandle* credentialHandle, System.Void* inContextPtr, System.Byte* targetName, System.Net.ContextFlags inFlags, System.Int32 reservedI, System.Net.Endianness endianness, System.Net.SecurityBufferDescriptor inputBuffer, System.Int32 reservedII, System.Net.SSPIHandle* outContextPtr, System.Net.SecurityBufferDescriptor outputBuffer, System.Net.ContextFlags* attributes, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.InitializeSecurityContextA' has not been implemented!");
		}

		public static System.Int32 AcceptSecurityContext(System.Net.SSPIHandle* credentialHandle, System.Void* inContextPtr, System.Net.SecurityBufferDescriptor inputBuffer, System.Net.ContextFlags inFlags, System.Net.Endianness endianness, System.Net.SSPIHandle* outContextPtr, System.Net.SecurityBufferDescriptor outputBuffer, System.Net.ContextFlags* attributes, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.AcceptSecurityContext' has not been implemented!");
		}

		public static System.Int32 DeleteSecurityContext(System.Net.SSPIHandle* handlePtr)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.DeleteSecurityContext' has not been implemented!");
		}

		public static System.Int32 FreeCredentialsHandle(System.Net.SSPIHandle* handlePtr)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.FreeCredentialsHandle' has not been implemented!");
		}

		public static System.Int32 EnumerateSecurityPackagesA(System.Int32* pkgnum, System.Net.SafeFreeContextBuffer_SCHANNEL* handle)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.EnumerateSecurityPackagesA' has not been implemented!");
		}

		public static System.Int32 AcquireCredentialsHandleA(System.String principal, System.String moduleName, System.Int32 usage, System.Void* logonID, System.Net.SecureCredential* authData, System.Void* keyCallback, System.Void* keyArgument, System.Net.SSPIHandle* handlePtr, System.Int64* timeStamp)
		{
			throw new System.NotImplementedException("Method 'System.Net.UnsafeNclNativeMethods+SafeNetHandles_SCHANNEL.AcquireCredentialsHandleA' has not been implemented!");
		}
	}
}
