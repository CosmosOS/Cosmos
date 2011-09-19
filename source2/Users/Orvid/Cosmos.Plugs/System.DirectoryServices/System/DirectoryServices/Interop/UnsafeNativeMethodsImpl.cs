namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DirectoryServices.Interop.UnsafeNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_DirectoryServices_Interop_UnsafeNativeMethodsImpl
	{

		public static System.Int32 IntADsOpenObject(System.String path, System.String userName, System.String password, System.Int32 flags, System.Guid* iid, System.Object* ppObject)
		{
			throw new System.NotImplementedException("Method 'System.DirectoryServices.Interop.UnsafeNativeMethods.IntADsOpenObject' has not been implemented!");
		}
	}
}
