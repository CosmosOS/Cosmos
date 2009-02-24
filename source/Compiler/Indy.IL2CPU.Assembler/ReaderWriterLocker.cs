using System;
using System.Linq;
using System.Threading;

namespace Indy.IL2CPU.Assembler {
    /// <summary>
    /// <see cref="ReaderWriterLocker"/> provides an easier interface to <see cref="ReaderWriterLockSlim"/>. It wraps Acquire/Release pairs using an IDisposable
    /// implementation, and automatically upgrades reader locks, if neccessary.
    /// </summary>
    public sealed class ReaderWriterLocker {
        private readonly ReaderWriterLock mLock = new ReaderWriterLock();

        /// <summary>
        /// Returns whether or not a writer lock is currently held.
        /// </summary>
        public bool WriterLockHeld {
            get {
                return mLock.IsWriterLockHeld;
            }
        }

        /// <summary>
        /// Returns whether or not a lock is currently held.
        /// </summary>
        public bool ReaderLockHeld {
            get {
                return mLock.IsReaderLockHeld || mLock.IsWriterLockHeld;
            }
        }

        /// <summary>
        /// Acquires a reader lock, if there's currently no lock held. 
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> implementation, which releases the reader lock on disposal, or null if there was
        /// already a reader lock held.</returns>
        public IDisposable AcquireReaderLock() {
            if (mLock.IsReaderLockHeld || mLock.IsWriterLockHeld) {
                return null;
            }
            mLock.AcquireReaderLock(-1);
            return new ReadUnlocker(mLock);
        }

        /// <summary>
        /// Acquires a writer lock, if there's currently no lock held. 
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> implementation, which restores the lock state on disposal, or null if there was
        /// already a reader lock held.</returns>
        public IDisposable AcquireWriterLock() {
            if (mLock.IsWriterLockHeld) {
                return null;
            }
            if (mLock.IsReaderLockHeld) {
                return new UpgradedReadLock(mLock);
            }
            mLock.AcquireWriterLock(-1);
            return new WriteUnlocker(mLock);
        }

        private sealed class ReadUnlocker : IDisposable {
            private readonly ReaderWriterLock mLock;

            public ReadUnlocker(ReaderWriterLock theLock) {
                mLock = theLock;
            }

            public void Dispose() {
                mLock.ReleaseReaderLock();
            }
        }

        private sealed class UpgradedReadLock : IDisposable {
            private readonly ReaderWriterLock mLock;

            public UpgradedReadLock(ReaderWriterLock aLock) {
                mLock = aLock;
                aLock.ReleaseReaderLock();
                aLock.AcquireWriterLock(-1);
            }

            public void Dispose() {
                mLock.ReleaseWriterLock();
                mLock.AcquireReaderLock(-1);
            }
        }

        private sealed class WriteUnlocker : IDisposable {
            private readonly ReaderWriterLock mLock;

            public WriteUnlocker(ReaderWriterLock theLock) {
                mLock = theLock;
            }

            public void Dispose() {
                mLock.ReleaseWriterLock();
            }
        }
    }
}