#define COSMOSDEBUG

using System;
using System.IO;

using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    internal class FatStream : Stream
    {
        protected byte[] mReadBuffer;

        protected ulong? mReadBufferPosition;

        protected ulong mPosition;

        private readonly FatDirectoryEntry mDirectoryEntry;

        private readonly FatFileSystem mFS;

        //TODO: In future we might read this in as needed rather than
        // all at once. This structure will also consume 2% of file size in RAM
        // (for default cluster size of 2kb, ie 4 bytes per cluster)
        // so we might consider a way to flush it and only keep parts.
        // Example, a 100 MB file will require 2MB for this structure. That is
        // probably acceptable for the mid term future.
        private readonly ulong[] mFatTable;

        private ulong mSize;

        public FatStream(FatDirectoryEntry aEntry)
        {
            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }

            mDirectoryEntry = aEntry;
            mFS = aEntry.GetFileSystem();
            mFatTable = aEntry.GetFatTable();
            mSize = aEntry.mSize;

            if (mFatTable == null)
            {
                throw new Exception("The fat chain returned for the directory entry was null.");
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public sealed override long Length
        {
            get
            {
                Global.mFileSystemDebugger.SendInternal("-- FatStream.get_Length --");
                Global.mFileSystemDebugger.SendInternal("Length =");
                Global.mFileSystemDebugger.SendInternal(mSize);
                return (long)mSize;
            }
        }

        public override long Position
        {
            get
            {
                Global.mFileSystemDebugger.SendInternal("-- FatStream.get_Position --");
                Global.mFileSystemDebugger.SendInternal("Position =");
                Global.mFileSystemDebugger.SendInternal(mPosition);
                return (long)mPosition;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                Global.mFileSystemDebugger.SendInternal("-- FatStream.set_Position --");
                Global.mFileSystemDebugger.SendInternal("Position =");
                Global.mFileSystemDebugger.SendInternal(mPosition);
                mPosition = (ulong)value;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatStream.SetLength --");
            Global.mFileSystemDebugger.SendInternal("value =");
            Global.mFileSystemDebugger.SendInternal(value);

            mDirectoryEntry.SetSize(value);
            mSize = (ulong)value;
        }

        public override int Read(byte[] aBuffer, int aOffset, int aCount)
        {
            return Read(aBuffer, aOffset, aCount);
        }

        public override void Write(byte[] aBuffer, int aOffset, int aCount)
        {
            Write(aBuffer, aOffset, aCount);
        }

        protected int Read(byte[] aBuffer, long aOffset, long aCount)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatStream.Read --");
            Global.mFileSystemDebugger.SendInternal("aBuffer.Length =");
            Global.mFileSystemDebugger.SendInternal(aBuffer.Length);
            Global.mFileSystemDebugger.SendInternal("aOffset =");
            Global.mFileSystemDebugger.SendInternal(aOffset);
            Global.mFileSystemDebugger.SendInternal("aCount =");
            Global.mFileSystemDebugger.SendInternal(aCount);

            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aCount));
            }

            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aOffset));
            }

            if (aCount + aOffset > aBuffer?.Length)
            {
                throw new ArgumentException("Invalid offset length.");
            }

            if (mFatTable.Length == 0 || mFatTable[0] == 0)
            {
                return 0;
            }

            if (mPosition >= mDirectoryEntry.mSize)
            {
                return 0;
            }

            ulong xMaxReadableBytes = mDirectoryEntry.mSize - mPosition;
            ulong xCount = (ulong)aCount;
            if (xCount > xMaxReadableBytes)
            {
                xCount = xMaxReadableBytes;
            }

            uint xClusterSize = mFS.BytesPerCluster;

            while (xCount > 0)
            {
                ulong xClusterIdx = mPosition / xClusterSize;
                ulong xPosInCluster = mPosition % xClusterSize;
                byte[] xCluster;
                mFS.Read(mFatTable[(int)xClusterIdx], out xCluster);
                long xReadSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xReadSize = (long)(xClusterSize - xPosInCluster - 1);
                }
                else
                {
                    xReadSize = (long)xCount;
                }

                Array.Copy(xCluster, (long)xPosInCluster, aBuffer, aOffset, xReadSize);

                aOffset += xReadSize;
                xCount -= (ulong)xReadSize;
            }

            mPosition += (ulong)aOffset;
            return (int)aOffset;
        }

        protected void Write(byte[] aBuffer, long aOffset, long aCount)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatStream.Write --");
            Global.mFileSystemDebugger.SendInternal("aBuffer.Length =");
            Global.mFileSystemDebugger.SendInternal(aBuffer.Length);
            Global.mFileSystemDebugger.SendInternal("aOffset =");
            Global.mFileSystemDebugger.SendInternal(aOffset);
            Global.mFileSystemDebugger.SendInternal("aCount =");
            Global.mFileSystemDebugger.SendInternal(aCount);

            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aCount));
            }

            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aOffset));
            }

            if (aOffset + aCount > aBuffer.Length)
            {
                throw new ArgumentException("Invalid offset length.");
            }

            ulong xCount = (ulong)aCount;
            uint xClusterSize = mFS.BytesPerCluster;

            long xTotalLength = (long)(mPosition + xCount);
            if (xTotalLength > Length)
            {
                SetLength(xTotalLength);
            }

            while (xCount > 0)
            {
                long xWriteSize;
                ulong xClusterIdx = mPosition / xClusterSize;
                ulong xPosInCluster = mPosition % xClusterSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xWriteSize = (long)(xClusterSize - xPosInCluster - 1);
                }
                else
                {
                    xWriteSize = (long)xCount;
                }

                byte[] xCluster;
                mFS.Read(xClusterIdx, out xCluster);
                Array.Copy(aBuffer, aOffset, xCluster, (long)xPosInCluster, xWriteSize);
                mFS.Write(mFatTable[(int)xClusterIdx], xCluster);
                aOffset += xWriteSize;
                xCount -= (ulong)xWriteSize;
            }

            mPosition += (ulong)aOffset;
        }
    }
}
