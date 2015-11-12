using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    public class FatStream : Stream
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
        protected List<UInt64> mFatTable;

        protected UInt64? mReadBufferPosition;

        protected UInt64 mPosition;

        private ulong mSize;

        public FatStream(FatDirectoryEntry aFile)
        {
            mDirectoryEntry = aFile;
            mFS = mDirectoryEntry.FileSystem;
            mReadBuffer = mDirectoryEntry.FileSystem.NewClusterArray();

            mSize = this.mDirectoryEntry.Size;
            if (mDirectoryEntry.Size > 0)
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
                FatHelpers.Debug("Retrieving size from DirectoryEntry");
                if (mDirectoryEntry == null)
                {
                    FatHelpers.Debug("No DirectoryEntry!");
                }
                FatHelpers.DebugNumber(mSize);
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

        protected int Read(byte[] aBuffer, Int64 aOffset, Int64 aCount)
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
            if (mDirectoryEntry.FirstClusterNum == 0)
            {
                // FirstSector can be 0 for 0 length files
                return 0;
            }
            if (mPosition == mDirectoryEntry.Size)
            {
                // EOF
                return 0;
            }

            // reduce count, so that no out of bound exception occurs if not existing
            // entry is used in line mFS.ReadCluster(mFatTable[(int)xClusterIdx], xCluster);
            ulong xMaxReadableBytes = mDirectoryEntry.Size - mPosition;
            ulong xCount = (ulong)aCount;
            if (xCount > xMaxReadableBytes)
            {
                xCount = xMaxReadableBytes;
            }

            var xCluster = mFS.NewClusterArray();
            UInt32 xClusterSize = mFS.BytesPerCluster;

            while (xCount > 0)
            {
                UInt64 xClusterIdx = mPosition / xClusterSize;
                UInt64 xPosInCluster = mPosition % xClusterSize;
                mFS.ReadCluster((ulong)mFatTable[(int)xClusterIdx], xCluster);
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
            var length = Length;
            var bytesPerCluser = mFS.BytesPerCluster;
            var xOldClusterTotal = length / bytesPerCluser;
            var xNewClusterTotal = value / bytesPerCluser;

            if (MathEx.Rem(length, bytesPerCluser) != 0)
            {
                xOldClusterTotal++;
            }

            if (MathEx.Rem(value, bytesPerCluser) != 0)
            {
                xNewClusterTotal++;
            }

            if (xNewClusterTotal != xOldClusterTotal)
            {
                throw new NotImplementedException(
                    "Setting the stream length to a size that requires alllcating new clusters is not currently implemented.");
            }

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
            UInt32 xClusterSize = mFS.BytesPerCluster;

            long xTotalLength = (long)(mPosition + xCount);
            if (xTotalLength > Length)
            {
                SetLength(xTotalLength);
            }

            while (xCount > 0)
            {
                long xWriteSize;
                UInt64 xClusterIdx = mPosition / xClusterSize;
                UInt64 xPosInCluster = mPosition % xClusterSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xWriteSize = (long)(xClusterSize - xPosInCluster - 1);
                }
                else
                {
                    xWriteSize = (long)xCount;
                }

                mFS.ReadCluster(xClusterIdx, xCluster);

                FatHelpers.Debug("Writing to cluster idx");
                FatHelpers.DebugNumber((uint)xClusterIdx);
                FatHelpers.Debug("Writing to pos in cluster");
                FatHelpers.DebugNumber((uint)xPosInCluster);
                FatHelpers.Debug("Offset");
                FatHelpers.DebugNumber((uint)aOffset);
                FatHelpers.Debug("xWriteSize");
                FatHelpers.DebugNumber((uint)xWriteSize);
                FatHelpers.Debug("First byte");
                FatHelpers.DebugNumber((uint)aBuffer[0]);

                //Array.Copy(aBuffer, (long)xPosInCluster, xCluster, aOffset, xWriteSize);
                Array.Copy(aBuffer, aOffset, xCluster, (long)xPosInCluster, xWriteSize);

                mFS.WriteCluster((ulong)mFatTable[(int)xClusterIdx], xCluster);

                aOffset += xWriteSize;
                xCount -= (ulong)xWriteSize;
            }

            mPosition += (ulong)aOffset;
        }
    }
}