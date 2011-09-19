namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.GC), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_GCImpl
	{

		public static System.Int32 GetGCLatencyMode()
		{
			throw new System.NotImplementedException("Method 'System.GC.GetGCLatencyMode' has not been implemented!");
		}

		public static System.Void SetGCLatencyMode(System.Int32 newLatencyMode)
		{
			throw new System.NotImplementedException("Method 'System.GC.SetGCLatencyMode' has not been implemented!");
		}

		public static System.Void _Collect(System.Int32 generation, System.Int32 mode)
		{
			throw new System.NotImplementedException("Method 'System.GC._Collect' has not been implemented!");
		}

		public static System.Int32 GetMaxGeneration()
		{
			throw new System.NotImplementedException("Method 'System.GC.GetMaxGeneration' has not been implemented!");
		}

		public static System.Int32 _CollectionCount(System.Int32 generation)
		{
			throw new System.NotImplementedException("Method 'System.GC._CollectionCount' has not been implemented!");
		}

		public static System.Boolean IsServerGC()
		{
			throw new System.NotImplementedException("Method 'System.GC.IsServerGC' has not been implemented!");
		}

		public static System.Int32 GetGeneration(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.GC.GetGeneration' has not been implemented!");
		}

		public static System.Void KeepAlive(System.Object obj)
		{
			throw new System.NotImplementedException("Method 'System.GC.KeepAlive' has not been implemented!");
		}

		public static System.Void _WaitForPendingFinalizers()
		{
			throw new System.NotImplementedException("Method 'System.GC._WaitForPendingFinalizers' has not been implemented!");
		}

		public static System.Void _SuppressFinalize(System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.GC._SuppressFinalize' has not been implemented!");
		}

		public static System.Int32 _WaitForFullGCApproach(System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.GC._WaitForFullGCApproach' has not been implemented!");
		}

		public static System.Int32 _WaitForFullGCComplete(System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.GC._WaitForFullGCComplete' has not been implemented!");
		}

		public static System.Int32 GetGenerationWR(System.IntPtr handle)
		{
			throw new System.NotImplementedException("Method 'System.GC.GetGenerationWR' has not been implemented!");
		}

		public static System.Int64 GetTotalMemory()
		{
			throw new System.NotImplementedException("Method 'System.GC.GetTotalMemory' has not been implemented!");
		}

		public static System.Void _AddMemoryPressure(System.UInt64 bytesAllocated)
		{
			throw new System.NotImplementedException("Method 'System.GC._AddMemoryPressure' has not been implemented!");
		}

		public static System.Void _RemoveMemoryPressure(System.UInt64 bytesAllocated)
		{
			throw new System.NotImplementedException("Method 'System.GC._RemoveMemoryPressure' has not been implemented!");
		}

		public static System.Void _ReRegisterForFinalize(System.Object o)
		{
			throw new System.NotImplementedException("Method 'System.GC._ReRegisterForFinalize' has not been implemented!");
		}

		public static System.Boolean _RegisterForFullGCNotification(System.Int32 maxGenerationPercentage, System.Int32 largeObjectHeapPercentage)
		{
			throw new System.NotImplementedException("Method 'System.GC._RegisterForFullGCNotification' has not been implemented!");
		}

		public static System.Boolean _CancelFullGCNotification()
		{
			throw new System.NotImplementedException("Method 'System.GC._CancelFullGCNotification' has not been implemented!");
		}

		public static System.Void SetCleanupCache()
		{
			throw new System.NotImplementedException("Method 'System.GC.SetCleanupCache' has not been implemented!");
		}
	}
}
