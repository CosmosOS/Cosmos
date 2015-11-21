using System;
using System.IO;

using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    internal class FatStream : Stream
    {
        protected readonly FatDirectoryEntry mDirectoryEntry;

        protected readonly FatFileSystem mFS;

        protected byte[] mReadBuffer;

        //TODO: In future we might read this in as needed rather than
        // all at once. This structure will also consume 2% of file size in RAM 
        // (for default cluster size of 2kb, ie 4 bytes per cluster)
        // so we might consider a way to flush it and only keep parts.
        // Example, a 100 MB file will require 2MB for this structure. That is
        // probably acceptable for the mid term future.
        protected ulong[] mFatTable;

        protected ulong? mReadBufferPosition;

        protected ulong mPosition;

        private ulong mSize;

        public FatStream(FatDirectoryEntry aEntry)
        {
            mDirectoryEntry = aEntry;
            mFS = mDirectoryEntry.GetFileSystem();
            mSize = mDirectoryEntry.mSize;
            if (mDirectoryEntry.mSize > 0)
            {
                mFatTable = mDirectoryEntry.GetFatTable();
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
                //FatHelpers.Debug("Retrieving size from DirectoryEntry");
                if (mDirectoryEntry == null)
                {
                    throw new NullReferenceException("The stream does not currently have an open entry.");
                    //FatHelpers.Debug("No DirectoryEntry!");
                }
                //FatHelpers.DebugNumber(mSize);
                return (long)mSize;
            }
        }

        public override long Position
        {
            get
            {
                return (long)mPosition;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                mPosition = (ulong)value;
            }
        }

        public override int Read(byte[] aBuffer, int aOffset, int aCount)
        {
            return Read(aBuffer, aOffset, aCount);
        }

        protected int Read(byte[] aBuffer, long aOffset, long aCount)
        {
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException("aCount");
            }
            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException("aOffset");
            }
            if (aBuffer == null || aBuffer.Length - aOffset < aCount)
            {
                throw new ArgumentException("Invalid offset length!");
            }
            if (mFatTable.Length == 0 || mFatTable[0] == 0)
            {
                // FirstSector can be 0 for 0 length files
                return 0;
            }
            if (mPosition == mDirectoryEntry.mSize)
            {
                // EOF
                return 0;
            }

            // reduce count, so that no out of bound exception occurs if not existing
            // entry is used in line mFS.ReadCluster(mFatTable[(int)xClusterIdx], xCluster);
            ulong xMaxReadableBytes = mDirectoryEntry.mSize - mPosition;
            ulong xCount = (ulong)aCount;
            if (xCount > xMaxReadableBytes)
            {
                xCount = xMaxReadableBytes;
            }

            var xCluster = mFS.NewClusterArray();
            uint xClusterSize = mFS.BytesPerCluster;

            while (xCount > 0)
            {
                ulong xClusterIdx = mPosition / xClusterSize;
                ulong xPosInCluster = mPosition % xClusterSize;
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
                // no need for a long version, because internal Array.Copy() does a cast down to int, and a range check,
                // or we do a semantic change here
                Array.Copy(xCluster, (long)xPosInCluster, aBuffer, aOffset, xReadSize);

                aOffset += xReadSize;
                xCount -= (ulong)xReadSize;
            }

            mPosition += (ulong)aOffset;
            return (int)aOffset;
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
            mDirectoryEntry.SetSize(value);
            mSize = (ulong)value;
        }

        public override void Write(byte[] aBuffer, int aOffset, int aCount)
        {
            Write(aBuffer, aOffset, aCount);
        }

        protected void Write(byte[] aBuffer, long aOffset, long aCount)
        {
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException("aCount");
            }
            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException("aOffset");
            }
            if (aBuffer == null || aBuffer.Length - aOffset < aCount)
            {
                throw new ArgumentException("Invalid offset length!");
            }

            ulong xCount = (ulong)aCount;
            var xCluster = mFS.NewClusterArray();
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

                mFS.Read(xClusterIdx, out xCluster);

                //FatHelpers.Debug("Writing to cluster idx");
                //FatHelpers.DebugNumber((uint)xClusterIdx);
                //FatHelpers.Debug("Writing to pos in cluster");
                //FatHelpers.DebugNumber((uint)xPosInCluster);
                //FatHelpers.Debug("Offset");
                //FatHelpers.DebugNumber((uint)aOffset);
                //FatHelpers.Debug("xWriteSize");
                //FatHelpers.DebugNumber((uint)xWriteSize);
                //FatHelpers.Debug("First byte");
                //FatHelpers.DebugNumber((uint)aBuffer[0]);

                Array.Copy(aBuffer, aOffset, xCluster, (long)xPosInCluster, xWriteSize);

                mFS.Write(mFatTable[(int)xClusterIdx], xCluster);

                aOffset += xWriteSize;
                xCount -= (ulong)xWriteSize;
            }

            mPosition += (ulong)aOffset;
        }
    }
}