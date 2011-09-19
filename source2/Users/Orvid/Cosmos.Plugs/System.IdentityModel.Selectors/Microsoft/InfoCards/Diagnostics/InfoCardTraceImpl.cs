namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.InfoCards.Diagnostics.InfoCardTrace), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class Microsoft_InfoCards_Diagnostics_InfoCardTraceImpl
	{

		public static System.Boolean ReportEvent(System.Runtime.InteropServices.SafeHandle hEventLog, System.Int16 type, System.UInt16 category, System.UInt32 eventID, System.Byte[] userSID, System.Int16 numStrings, System.Int32 dataLen, System.Runtime.InteropServices.HandleRef strings, System.Byte[] rawData)
		{
			throw new System.NotImplementedException("Method 'Microsoft.InfoCards.Diagnostics.InfoCardTrace.ReportEvent' has not been implemented!");
		}
	}
}
