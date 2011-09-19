namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.CompilerServices.DependentHandle), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_CompilerServices_DependentHandleImpl
	{

		public static System.Void nInitialize(System.Object primary, System.Object secondary, System.IntPtr* dependentHandle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.CompilerServices.DependentHandle.nInitialize' has not been implemented!");
		}

		public static System.Void nGetPrimary(System.IntPtr dependentHandle, System.Object* primary)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.CompilerServices.DependentHandle.nGetPrimary' has not been implemented!");
		}

		public static System.Void nGetPrimaryAndSecondary(System.IntPtr dependentHandle, System.Object* primary, System.Object* secondary)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.CompilerServices.DependentHandle.nGetPrimaryAndSecondary' has not been implemented!");
		}

		public static System.Void nFree(System.IntPtr dependentHandle)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.CompilerServices.DependentHandle.nFree' has not been implemented!");
		}
	}
}
