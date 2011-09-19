namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.DefaultBinder), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_DefaultBinderImpl
	{

		public static System.Boolean CanConvertPrimitiveObjectToType(System.Object source, System.RuntimeType type)
		{
			throw new System.NotImplementedException("Method 'System.DefaultBinder.CanConvertPrimitiveObjectToType' has not been implemented!");
		}

		public static System.Boolean CanConvertPrimitive(System.RuntimeType source, System.RuntimeType target)
		{
			throw new System.NotImplementedException("Method 'System.DefaultBinder.CanConvertPrimitive' has not been implemented!");
		}
	}
}
