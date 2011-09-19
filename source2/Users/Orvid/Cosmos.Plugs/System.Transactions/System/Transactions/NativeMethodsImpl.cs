namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Transactions.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Transactions_NativeMethodsImpl
	{

		public static System.Void CoGetContextToken(System.IntPtr* contextToken)
		{
			throw new System.NotImplementedException("Method 'System.Transactions.NativeMethods.CoGetContextToken' has not been implemented!");
		}

		public static System.Void CoGetDefaultContext(System.Int32 aptType, System.Guid* contextInterface, System.Transactions.SafeIUnknown* safeUnknown)
		{
			throw new System.NotImplementedException("Method 'System.Transactions.NativeMethods.CoGetDefaultContext' has not been implemented!");
		}
	}
}
