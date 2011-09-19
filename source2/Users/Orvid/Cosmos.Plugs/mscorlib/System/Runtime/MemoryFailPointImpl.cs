namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Runtime.MemoryFailPoint), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Runtime_MemoryFailPointImpl
	{

		public static System.Void GetMemorySettings(System.UInt32* maxGCSegmentSize, System.UInt64* topOfMemory)
		{
			throw new System.NotImplementedException("Method 'System.Runtime.MemoryFailPoint.GetMemorySettings' has not been implemented!");
		}
	}
}
