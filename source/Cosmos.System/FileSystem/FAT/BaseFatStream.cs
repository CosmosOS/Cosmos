using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    public class BaseFatStream : Stream
    {
        private readonly Base mFileOrDirectory;
        private readonly FatFileSystem mFileSystem;
        private readonly ulong mFirstClusterNumber;

        //TODO: In future we might read this in as needed rather than
        // all at once. This structure will also consume 2% of file size in RAM
        // (for default cluster size of 2kb, ie 4 bytes per cluster)
        // so we might consider a way to flush it and only keep parts.
        // Example, a 100 MB file will require 2MB for this structure. That is
        // probably acceptable for the mid term future.
        protected List<UInt64> mFatTable;

        protected BaseFatStream(Base fileOrDirectory, FatFileSystem fileSystem, ulong firstClusterNumber)
        {
            mFileOrDirectory = fileOrDirectory;
            mFirstClusterNumber = firstClusterNumber;
            mFileSystem = fileSystem;

            mSize = mFileOrDirectory.Size;
            if (mSize > 0)
            {
                mFatTable = mFileSystem.GetFatTable(firstClusterNumber);
            }
        }

        public override sealed bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override sealed bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override sealed bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override sealed long Length
        {
            get
            {
                FatHelpers.Debug("Retrieving size from FileOrDirectory");
                if (mFileOrDirectory == null)
                {
                    FatHelpers.Debug("No FileOrDirectory!");
                }
                return (long) mSize;
            }
        }

        protected UInt64 mPosition;
        private ulong mSize;

        public override sealed long Position
        {
            get
            {
                return (long) mPosition;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                mPosition = (ulong) value;
            }
        }

        public override sealed int Read(byte[] aBuffer, int aOffset, int aCount)
        {
            return Read(aBuffer, aOffset, aCount);
        }

        protected int Read(byte[] aBuffer, Int64 aOffset, Int64 aCount)
        {
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aCount));
            }
            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aOffset));
            }
            if (aBuffer == null || aBuffer.Length - aOffset < aCount)
            {
                throw new ArgumentException("Invalid offset length!");
            }
            if (mFirstClusterNumber == 0)
            {
                // FirstSector can be 0 for 0 length files
                return 0;
            }
            if (mPosition == mSize)
            {
                // EOF
                return 0;
            }

            // reduce count, so that no out of bound exception occurs if not existing
            // entry is used in line mFS.ReadCluster(mFatTable[(int)xClusterIdx], xCluster);
            ulong xMaxReadableBytes = mSize - mPosition;
            ulong xCount = (ulong) aCount;
            if (xCount > xMaxReadableBytes)
            {
                xCount = xMaxReadableBytes;
            }

            var xCluster = mFileSystem.NewClusterArray();
            UInt32 xClusterSize = mFileSystem.BytesPerCluster;

            while (xCount > 0)
            {
                UInt64 xClusterIdx = mPosition / xClusterSize;
                UInt64 xPosInCluster = mPosition % xClusterSize;
                mFileSystem.ReadCluster((ulong) mFatTable[(int) xClusterIdx], xCluster);
                long xReadSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xReadSize = (long) (xClusterSize - xPosInCluster - 1);
                }
                else
                {
                    xReadSize = (long) xCount;
                }


                // no need for a long version, because internal Array.Copy() does a cast down to int, and a range check,
                // or we do a semantic change here
                Array.Copy(xCluster, (long) xPosInCluster, aBuffer, aOffset, xReadSize);

                aOffset += xReadSize;
                xCount -= (ulong) xReadSize;
            }

            mPosition += (ulong) aOffset;
            return (int) aOffset;
        }

        public override sealed void Flush()
        {
            throw new NotImplementedException();
        }

        public override sealed long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override sealed void SetLength(long value)
        {
            var xOldClusterTotal = Length / mFileSystem.BytesPerCluster;
            if (Length % mFileSystem.BytesPerCluster != 0)
            {
                xOldClusterTotal++;
            }

            var xNewClusterTotal = value / mFileSystem.BytesPerCluster;
            if (value % mFileSystem.BytesPerCluster != 0)
            {
                xNewClusterTotal++;
            }

            if (xNewClusterTotal != xOldClusterTotal)
            {
                throw new NotImplementedException("Setting the stream length to a size that requires alllcating new clusters is not currently implemented.");
            }

            mFileSystem.SetFileLength(mFileOrDirectory, value);
            //mDirectory.Size = (ulong) value;
        }

        public override sealed void Write(byte[] aBuffer, int aOffset, int aCount)
        {
            Write(aBuffer, aOffset, aCount);
        }

        protected void Write(byte[] aBuffer, long aOffset, long aCount)
        {
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aCount));
            }
            if (aOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aOffset));
            }
            if (aBuffer == null || aBuffer.Length - aOffset < aCount)
            {
                throw new ArgumentException("Invalid offset length!");
            }

            ulong xCount = (ulong) aCount;
            var xCluster = mFileSystem.NewClusterArray();
            UInt32 xClusterSize = mFileSystem.BytesPerCluster;

            long xTotalLength = (long) (mPosition + xCount);
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
                    xWriteSize = (long) (xClusterSize - xPosInCluster - 1);
                }
                else
                {
                    xWriteSize = (long) xCount;
                }

                mFileSystem.ReadCluster(xClusterIdx, xCluster);

                FatHelpers.Debug("Writing to cluster idx");
                FatHelpers.DebugNumber((uint) xClusterIdx);
                FatHelpers.Debug("Writing to pos in cluster");
                FatHelpers.DebugNumber((uint) xPosInCluster);
                FatHelpers.Debug("Offset");
                FatHelpers.DebugNumber((uint) aOffset);
                FatHelpers.Debug("xWriteSize");
                FatHelpers.DebugNumber((uint) xWriteSize);
                FatHelpers.Debug("First byte");
                FatHelpers.DebugNumber((uint) aBuffer[0]);

                Array.Copy(aBuffer, aOffset, xCluster, (long) xPosInCluster, xWriteSize);
                mFileSystem.WriteCluster(mFatTable[(int) xClusterIdx], xCluster);

                aOffset += xWriteSize;
                xCount -= (ulong) xWriteSize;
            }

            mPosition += (ulong) aOffset;

        }
    }
}
