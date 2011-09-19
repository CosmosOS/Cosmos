namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.StubHelpers.DateMarshaler), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_StubHelpers_DateMarshalerImpl
	{

		public static System.Double ConvertToNative(System.DateTime managedDate)
		{
			throw new System.NotImplementedException("Method 'System.StubHelpers.DateMarshaler.ConvertToNative' has not been implemented!");
		}

		public static System.Int64 ConvertToManaged(System.Double nativeDate)
		{
			throw new System.NotImplementedException("Method 'System.StubHelpers.DateMarshaler.ConvertToManaged' has not been implemented!");
		}
	}
}
