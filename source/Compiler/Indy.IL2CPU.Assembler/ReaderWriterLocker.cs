using System;
using System.Linq;
using System.Threading;

namespace Indy.IL2CPU.Assembler {
    /// <summary>
    /// <see cref="ReaderWriterLocker"/> provides an easier interface to <see cref="ReaderWriterLockSlim"/>. It wraps Acquire/Release pairs using an IDisposable
    /// implementation, and automatically upgrades reader locks, if neccessary.
    /// </summary>
    public sealed class ReaderWriterLocker {
        private readonly ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Returns whether or not a writer lock is currently held.
        /// </summary>
        public bool WriterLockHeld {
            get {
                return mLock.IsWriteLockHeld;
            }
        }

        /// <summary>
        /// Returns whether or not a lock is currently held.
        /// </summary>
        public bool ReaderLockHeld {
            get {
                return mLock.IsReadLockHeld || mLock.IsWriteLockHeld;
            }
        }

        /// <summary>
        /// Acquires a reader lock, if there's currently no lock held. 
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> implementation, which releases the reader lock on disposal, or null if there was
        /// already a reader lock held.</returns>
        public IDisposable AcquireReaderLock() {
            if (mLock.IsReadLockHeld || mLock.IsWriteLockHeld) {
                return null;
            }
            mLock.EnterReadLock();
            return new ReadUnlocker(mLock);
        }

        /// <summary>
        /// Acquires a writer lock, if there's currently no lock held. 
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> implementation, which restores the lock state on disposal, or null if there was
        /// already a reader lock held.</returns>
        public IDisposable AcquireWriterLock() {
            if (mLock.IsWriteLockHeld) {
                return null;
            }
            if (mLock.IsReadLockHeld) {
                return new UpgradedReadLock(mLock);
            }
            mLock.EnterWriteLock();
            return new WriteUnlocker(mLock);
        }

        private sealed class ReadUnlocker : IDisposable {
            private readonly ReaderWriterLockSlim mLock;

            public ReadUnlocker(ReaderWriterLockSlim theLock)
            {
                mLock = theLock;
            }

            public void Dispose() {
                mLock.ExitReadLock();
            }
        }

        private sealed class UpgradedReadLock : IDisposable {
            private readonly ReaderWriterLockSlim mLock;

            public UpgradedReadLock(ReaderWriterLockSlim aLock)
            {
                mLock = aLock;
                aLock.ExitReadLock();
                aLock.EnterWriteLock();
            }

            public void Dispose() {
                mLock.ExitWriteLock();
                mLock.EnterReadLock();
            }
        }

        private sealed class WriteUnlocker : IDisposable {
            private readonly ReaderWriterLockSlim mLock;

            public WriteUnlocker(ReaderWriterLockSlim theLock)
            {
                mLock = theLock;
            }

            public void Dispose() {
                mLock.ExitWriteLock();
            }
        }
    }
}