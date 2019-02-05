//#define COSMOSDEBUG

using System;
using System.IO;

using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    internal class FatStream : Stream
    {
        protected byte[] mReadBuffer;

        protected long? mReadBufferPosition;

        protected long mPosition;

        private readonly FatDirectoryEntry mDirectoryEntry;

        private readonly FatFileSystem mFS;

        //TODO: In future we might read this in as needed rather than
        // all at once. This structure will also consume 2% of file size in RAM
        // (for default cluster size of 2kb, ie 4 bytes per cluster)
        // so we might consider a way to flush it and only keep parts.
        // Example, a 100 MB file will require 2MB for this structure. That is
        // probably acceptable for the mid term future.
        private readonly uint[] mFatTable;

        private long mSize;

        public FatStream(FatDirectoryEntry aEntry)
        {
            mDirectoryEntry = aEntry ?? throw new ArgumentNullException(nameof(aEntry));
            mFS = aEntry.GetFileSystem();
            mFatTable = aEntry.GetFatTable();
            mSize = aEntry.mSize;

            if (mFatTable == null)
            {
                throw new Exception("The fat chain returned for the directory entry was null.");
            }
        }

        public override bool CanSeek => true;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public sealed override long Length
        {
            get
            {
                Global.mFileSystemDebugger.SendInternal("-- FatStream.get_Length --");
                Global.mFileSystemDebugger.SendInternal("Length =");
                Global.mFileSystemDebugger.SendInternal(mSize);
                return mSize;
            }
        }

        public override long Position
        {
            get
            {
                Global.mFileSystemDebugger.SendInternal("-- FatStream.get_Position --");
                Global.mFileSystemDebugger.SendInternal("Position =");
                Global.mFileSystemDebugger.SendInternal(mPosition);
                return mPosition;
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
                mPosition = value;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatStream.SetLength --");
            Global.mFileSystemDebugger.SendInternal("value =");
            Global.mFileSystemDebugger.SendInternal(value);

            mDirectoryEntry.SetSize(value);
            mSize = value;
        }

        public override int Read(byte[] aBuffer, int aOffset, int aCount)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatStream.Read --");
            Global.mFileSystemDebugger.SendInternal("aBuffer.Length =");
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

            long xMaxReadableBytes = mDirectoryEntry.mSize - mPosition;
            long xCount = aCount;
            long xOffset = aOffset;

            if (xCount > xMaxReadableBytes)
            {
                xCount = xMaxReadableBytes;
            }

            long xClusterSize = mFS.BytesPerCluster;
            Global.mFileSystemDebugger.SendInternal("cluster size" + xClusterSize);

            while (xCount > 0)
            {
                Global.mFileSystemDebugger.SendInternal("Position: " + mPosition + " Count: " + xCount + " Offset: " + xOffset);
                long xClusterIdx = mPosition / xClusterSize;
                long xPosInCluster = mPosition % xClusterSize;
                Global.mFileSystemDebugger.SendInternal("ClusterID: " + xClusterIdx + " Pos in Cluster: " + xPosInCluster);
                byte[] xCluster;
                mFS.Read(mFatTable[(int)xClusterIdx], out xCluster);
                long xReadSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xReadSize = xClusterSize - xPosInCluster; // -1
                }
                else
                {
                    xReadSize = xCount;
                }
                Global.mFileSystemDebugger.SendInternal("Cluster: " + xClusterIdx + "Read size: " + xReadSize);
                Array.Copy(xCluster, xPosInCluster, aBuffer, xOffset, xReadSize);

                xOffset += xReadSize;
                xCount -= xReadSize;
                mPosition += xReadSize;
            }

            return (int)xOffset;
        }

        public override void Write(byte[] aBuffer, int aOffset, int aCount)
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

            long xCount = aCount;
            long xClusterSize = mFS.BytesPerCluster;
            long xOffset = aOffset;

            long xTotalLength = (mPosition + xCount);

            if (xTotalLength > Length)
            {
                SetLength(xTotalLength);
            }

            while (xCount > 0)
            {
                long xWriteSize;
                long xClusterIdx = mPosition / xClusterSize;
                long xPosInCluster = mPosition % xClusterSize;
                Global.mFileSystemDebugger.SendInternal("Cluster id" + xClusterIdx + " Pos: " + mPosition + " Pos in Cluster: " + xPosInCluster);
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xWriteSize = xClusterSize - xPosInCluster;
                }
                else
                {
                    xWriteSize = xCount;
                }

                mFS.Read(mFatTable[xClusterIdx], out byte[] xCluster);
                Array.Copy(aBuffer, aOffset, xCluster, (int)xPosInCluster, (int)xWriteSize);
                mFS.Write(mFatTable[xClusterIdx], xCluster);

                Global.mFileSystemDebugger.SendInternal("xOffset: " + xOffset + " xCount: " + xCluster + " aOffset: " + aOffset + " mPosition: " + mPosition + " Write Size:" + xWriteSize);
                xOffset += xWriteSize;
                xCount -= xWriteSize;
                aOffset += (int)xWriteSize;
                mPosition += xWriteSize;
            }
        }
    }
}
