namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Security.Util.Config), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Security_Util_ConfigImpl
	{

		public static System.Int32 SaveDataByte(System.String path, System.Byte[] data, System.Int32 length)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.SaveDataByte' has not been implemented!");
		}

		public static System.Boolean RecoverData(System.Security.Policy.ConfigId id)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.RecoverData' has not been implemented!");
		}

		public static System.Void SetQuickCache(System.Security.Policy.ConfigId id, System.Security.Util.QuickCacheEntryType quickCacheFlags)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.SetQuickCache' has not been implemented!");
		}

		public static System.Void ResetCacheData(System.Security.Policy.ConfigId id)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.ResetCacheData' has not been implemented!");
		}

		public static System.Boolean WriteToEventLog(System.String message)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.WriteToEventLog' has not been implemented!");
		}

		public static System.Boolean GetCacheEntry(System.Security.Policy.ConfigId id, System.Int32 numKey, System.Byte[] key, System.Int32 keyLength, System.Runtime.CompilerServices.ObjectHandleOnStack retData)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.GetCacheEntry' has not been implemented!");
		}

		public static System.Void AddCacheEntry(System.Security.Policy.ConfigId id, System.Int32 numKey, System.Byte[] key, System.Int32 keyLength, System.Byte[] data, System.Int32 dataLength)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.AddCacheEntry' has not been implemented!");
		}

		public static System.Void GetMachineDirectory(System.Runtime.CompilerServices.StringHandleOnStack retDirectory)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.GetMachineDirectory' has not been implemented!");
		}

		public static System.Void GetUserDirectory(System.Runtime.CompilerServices.StringHandleOnStack retDirectory)
		{
			throw new System.NotImplementedException("Method 'System.Security.Util.Config.GetUserDirectory' has not been implemented!");
		}
	}
}
