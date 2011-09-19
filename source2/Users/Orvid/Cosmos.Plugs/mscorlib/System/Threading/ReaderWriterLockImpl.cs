namespace Cosmos.Plugs
{
	[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(System.Threading.ReaderWriterLock), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
	public static class System_Threading_ReaderWriterLockImpl
	{

		public static System.Void AcquireReaderLockInternal(System.Threading.ReaderWriterLock aThis, System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.AcquireReaderLockInternal' has not been implemented!");
		}

		public static System.Void AcquireWriterLockInternal(System.Threading.ReaderWriterLock aThis, System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.AcquireWriterLockInternal' has not been implemented!");
		}

		public static System.Void ReleaseReaderLockInternal(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.ReleaseReaderLockInternal' has not been implemented!");
		}

		public static System.Void ReleaseWriterLockInternal(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.ReleaseWriterLockInternal' has not been implemented!");
		}

		public static System.Void DowngradeFromWriterLockInternal(System.Threading.ReaderWriterLock aThis, System.Threading.LockCookie* lockCookie)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.DowngradeFromWriterLockInternal' has not been implemented!");
		}

		public static System.Boolean PrivateGetIsReaderLockHeld(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.PrivateGetIsReaderLockHeld' has not been implemented!");
		}

		public static System.Boolean PrivateGetIsWriterLockHeld(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.PrivateGetIsWriterLockHeld' has not been implemented!");
		}

		public static System.Void FCallUpgradeToWriterLock(System.Threading.ReaderWriterLock aThis, System.Threading.LockCookie* result, System.Int32 millisecondsTimeout)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.FCallUpgradeToWriterLock' has not been implemented!");
		}

		public static System.Void PrivateInitialize(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.PrivateInitialize' has not been implemented!");
		}

		public static System.Void PrivateDestruct(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.PrivateDestruct' has not been implemented!");
		}

		public static System.Void RestoreLockInternal(System.Threading.ReaderWriterLock aThis, System.Threading.LockCookie* lockCookie)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.RestoreLockInternal' has not been implemented!");
		}

		public static System.Int32 PrivateGetWriterSeqNum(System.Threading.ReaderWriterLock aThis)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.PrivateGetWriterSeqNum' has not been implemented!");
		}

		public static System.Boolean AnyWritersSince(System.Threading.ReaderWriterLock aThis, System.Int32 seqNum)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.AnyWritersSince' has not been implemented!");
		}

		public static System.Void FCallReleaseLock(System.Threading.ReaderWriterLock aThis, System.Threading.LockCookie* result)
		{
			throw new System.NotImplementedException("Method 'System.Threading.ReaderWriterLock.FCallReleaseLock' has not been implemented!");
		}
	}
}
