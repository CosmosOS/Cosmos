namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Configuration.Install.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Configuration_Install_NativeMethodsImpl
	{

		public static System.Int32 MsiCreateRecord(System.Int32 cParams)
		{
			throw new System.NotImplementedException("Method 'System.Configuration.Install.NativeMethods.MsiCreateRecord' has not been implemented!");
		}

		public static System.Int32 MsiRecordSetInteger(System.Int32 hRecord, System.Int32 iField, System.Int32 iValue)
		{
			throw new System.NotImplementedException("Method 'System.Configuration.Install.NativeMethods.MsiRecordSetInteger' has not been implemented!");
		}

		public static System.Int32 MsiRecordSetStringW(System.Int32 hRecord, System.Int32 iField, System.String szValue)
		{
			throw new System.NotImplementedException("Method 'System.Configuration.Install.NativeMethods.MsiRecordSetStringW' has not been implemented!");
		}

		public static System.Int32 MsiProcessMessage(System.Int32 hInstall, System.Int32 messageType, System.Int32 hRecord)
		{
			throw new System.NotImplementedException("Method 'System.Configuration.Install.NativeMethods.MsiProcessMessage' has not been implemented!");
		}
	}
}
