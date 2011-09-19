namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Policy.AssemblyEvidenceFactory), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Policy_AssemblyEvidenceFactoryImpl
	{

		public static System.Void GetStrongNameInformation(System.Reflection.RuntimeAssembly assembly, System.Runtime.CompilerServices.ObjectHandleOnStack retPublicKeyBlob, System.Runtime.CompilerServices.StringHandleOnStack retSimpleName, System.UInt16* majorVersion, System.UInt16* minorVersion, System.UInt16* build, System.UInt16* revision)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.AssemblyEvidenceFactory.GetStrongNameInformation' has not been implemented!");
		}

		public static System.Void GetAssemblyPermissionRequests(System.Reflection.RuntimeAssembly assembly, System.Runtime.CompilerServices.ObjectHandleOnStack retMinimumPermissions, System.Runtime.CompilerServices.ObjectHandleOnStack retOptionalPermissions, System.Runtime.CompilerServices.ObjectHandleOnStack retRefusedPermissions)
		{
			throw new System.NotImplementedException("Method 'System.Security.Policy.AssemblyEvidenceFactory.GetAssemblyPermissionRequests' has not been implemented!");
		}
	}
}
