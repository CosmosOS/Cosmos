namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.ThreadPool), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_ThreadPoolImpl
	{

		public static System.Boolean AdjustThreadsInPool(System.UInt32 QueueLength)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.AdjustThreadsInPool' has not been implemented!");
		}

		public static System.Boolean PostQueuedCompletionStatus(System.Threading.NativeOverlapped* overlapped)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.PostQueuedCompletionStatus' has not been implemented!");
		}

		public static System.Boolean ShouldUseNewWorkerPool()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.ShouldUseNewWorkerPool' has not been implemented!");
		}

		public static System.Boolean SetMinThreadsNative(System.Int32 workerThreads, System.Int32 completionPortThreads)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.SetMinThreadsNative' has not been implemented!");
		}

		public static System.Boolean SetMaxThreadsNative(System.Int32 workerThreads, System.Int32 completionPortThreads)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.SetMaxThreadsNative' has not been implemented!");
		}

		public static System.Void GetMinThreadsNative(System.Int32* workerThreads, System.Int32* completionPortThreads)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.GetMinThreadsNative' has not been implemented!");
		}

		public static System.Void GetMaxThreadsNative(System.Int32* workerThreads, System.Int32* completionPortThreads)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.GetMaxThreadsNative' has not been implemented!");
		}

		public static System.Void GetAvailableThreadsNative(System.Int32* workerThreads, System.Int32* completionPortThreads)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.GetAvailableThreadsNative' has not been implemented!");
		}

		public static System.Boolean CompleteThreadPoolRequest(System.UInt32 QueueLength)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.CompleteThreadPoolRequest' has not been implemented!");
		}

		public static System.Boolean NotifyWorkItemComplete()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.NotifyWorkItemComplete' has not been implemented!");
		}

		public static System.Void ReportThreadStatus(System.Boolean isWorking)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.ReportThreadStatus' has not been implemented!");
		}

		public static System.Void NotifyWorkItemProgressNative()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.NotifyWorkItemProgressNative' has not been implemented!");
		}

		public static System.Boolean ShouldReturnToVm()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.ShouldReturnToVm' has not been implemented!");
		}

		public static System.Boolean SetAppDomainRequestActive()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.SetAppDomainRequestActive' has not been implemented!");
		}

		public static System.Void ClearAppDomainRequestActive()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.ClearAppDomainRequestActive' has not been implemented!");
		}

		public static System.Boolean IsThreadPoolHosted()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.IsThreadPoolHosted' has not been implemented!");
		}

		public static System.Void SetNativeTpEvent()
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.SetNativeTpEvent' has not been implemented!");
		}

		public static System.Boolean BindIOCompletionCallbackNative(System.IntPtr fileHandle)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.BindIOCompletionCallbackNative' has not been implemented!");
		}

		public static System.Void UpdateNativeTpCount(System.UInt32 QueueLength)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.UpdateNativeTpCount' has not been implemented!");
		}

		public static System.Void InitializeVMTp(System.Boolean* enableWorkerTracking)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.InitializeVMTp' has not been implemented!");
		}

		public static System.IntPtr RegisterWaitForSingleObjectNative(System.Threading.WaitHandle waitHandle, System.Object state, System.UInt32 timeOutInterval, System.Boolean executeOnlyOnce, System.Threading.RegisteredWaitHandle registeredWaitHandle, System.Threading.StackCrawlMark* stackMark, System.Boolean compressStack)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ThreadPool.RegisterWaitForSingleObjectNative' has not been implemented!");
		}
	}
}
