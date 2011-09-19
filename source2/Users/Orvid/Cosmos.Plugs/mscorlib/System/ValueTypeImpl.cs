namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.ValueType), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_ValueTypeImpl
	{

		public static System.Int32 GetHashCode(System.ValueType aThis)
		{
			throw new System.NotImplementedException("Method 'System.ValueType.GetHashCode' has not been implemented!");
		}

		public static System.Int32 GetHashCodeOfPtr(System.IntPtr ptr)
		{
			throw new System.NotImplementedException("Method 'System.ValueType.GetHashCodeOfPtr' has not been implemented!");
		}

		public static System.Boolean CanCompareBits(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.ValueType.CanCompareBits' has not been implemented!");
		}

		public static System.Boolean FastEqualsCheck(System.Object a, System.Object b)
		{
			throw new System.NotImplementedException("Method 'System.ValueType.FastEqualsCheck' has not been implemented!");
		}
	}
}
