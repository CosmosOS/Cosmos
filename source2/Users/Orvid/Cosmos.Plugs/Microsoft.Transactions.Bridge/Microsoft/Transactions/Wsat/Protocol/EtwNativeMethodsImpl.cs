namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_Transactions_Wsat_Protocol_EtwNativeMethodsImpl
	{

		public static System.UInt32 GetTraceEnableFlags(System.UInt64 traceHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods.GetTraceEnableFlags' has not been implemented!");
		}

		public static System.Byte GetTraceEnableLevel(System.UInt64 traceHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods.GetTraceEnableLevel' has not been implemented!");
		}

		public static System.UInt32 TraceEvent(System.UInt64 traceHandle, System.Char* header)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods.TraceEvent' has not been implemented!");
		}

		public static System.UInt32 RegisterTraceGuids(Microsoft.Transactions.Wsat.Protocol.EtwTraceCallback cbFunc, System.Void* context, System.Guid* controlGuid, System.UInt32 guidCount, Microsoft.Transactions.Wsat.Protocol.TraceGuidRegistration* guidReg, System.String mofImagePath, System.String mofResourceName, System.UInt64* regHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods.RegisterTraceGuids' has not been implemented!");
		}

		public static System.Int32 UnregisterTraceGuids(System.UInt64 regHandle)
		{
			throw new System.NotImplementedException("Method 'Microsoft.Transactions.Wsat.Protocol.EtwNativeMethods.UnregisterTraceGuids' has not been implemented!");
		}
	}
}
