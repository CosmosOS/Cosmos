namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.CompressedStack), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_CompressedStackImpl
	{

		public static System.Threading.SafeCompressedStackHandle GetDelayedCompressedStack(System.Threading.StackCrawlMark* stackMark, System.Boolean walkStack)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.GetDelayedCompressedStack' has not been implemented!");
		}

		public static System.Void DestroyDCSList(System.Threading.SafeCompressedStackHandle compressedStack)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.DestroyDCSList' has not been implemented!");
		}

		public static System.Boolean IsImmediateCompletionCandidate(System.Threading.SafeCompressedStackHandle compressedStack, System.Threading.CompressedStack* innerCS)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.IsImmediateCompletionCandidate' has not been implemented!");
		}

		public static System.Void DestroyDelayedCompressedStack(System.IntPtr unmanagedCompressedStack)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.DestroyDelayedCompressedStack' has not been implemented!");
		}

		public static System.Int32 GetDCSCount(System.Threading.SafeCompressedStackHandle compressedStack)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.GetDCSCount' has not been implemented!");
		}

		public static System.Threading.DomainCompressedStack GetDomainCompressedStack(System.Threading.SafeCompressedStackHandle compressedStack, System.Int32 index)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.GetDomainCompressedStack' has not been implemented!");
		}

		public static System.Void GetHomogeneousPLS(System.Security.PermissionListSet hgPLS)
		{
			throw new System.NotImplementedException("Method 'System.Threading.CompressedStack.GetHomogeneousPLS' has not been implemented!");
		}
	}
}
