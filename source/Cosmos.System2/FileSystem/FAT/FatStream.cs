//#define COSMOSDEBUG

using System;
using System.IO;
using Cosmos.Core;
using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    /// <summary>
    /// FAT stream class.
    /// </summary>
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
        private uint[] mFatTable;

        private long mSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FatStream"/> class.
        /// </summary>
        /// <param name="aEntry">A directory entry to open stream to.</param>
        /// <exception cref="ArgumentNullException">Thrown when aEntry is null / corrupted.</exception>
        /// <exception cref="Exception">Thrown when FAT table not found or null / out of memory / invalid aData size.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size of the chain is less then zero.</exception>
        /// <exception cref="OverflowException">Thrown when the number of clusters in the FAT entry is greater than Int32.MaxValue</exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        /// <exception cref="ArgumentException">Thrown when FAT type is unknown.</exception>
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

        /// <summary>
        /// Check if can seek the stream.
        /// Returns true.
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// Check if can read the stream.
        /// Returns true.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Check if can write the stream.
        /// Returns true.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Get the length of the stream.
        /// </summary>
        public sealed override long Length
        {
            get
            {
                Global.Debugger.SendInternal("-- FatStream.get_Length --");
                Global.Debugger.SendInternal("Length =");
                Global.Debugger.SendInternal(mSize);
                return mSize;
            }
        }

        /// <summary>
        /// Get and set the position in the stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown when value is smaller than 0L.</exception>
        public override long Position
        {
            get
            {
                Global.Debugger.SendInternal("-- FatStream.get_Position --");
                Global.Debugger.SendInternal("Position =");
                Global.Debugger.SendInternal(mPosition);
                return mPosition;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                Global.Debugger.SendInternal("-- FatStream.set_Position --");
                Global.Debugger.SendInternal("Position =");
                Global.Debugger.SendInternal(mPosition);
                mPosition = value;
            }
        }

        /// <summary>
        /// Flush stream.
        /// Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seek the position in the stream.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The position in the stream to start the seek from.
        /// Possible values:
        /// <list type="bullet">
        /// <item>SeekOrigin.Begin</item>
        /// <item>SeekOrigin.Current</item>
        /// <item>SeekOrigin.End</item>
        /// </list>
        /// </param>
        /// <returns>long value.</returns>
        /// <exception cref="NotImplementedException">Thrown when invalid seek position in passed.</exception>
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

        /// <summary>
        /// Set the length of the stream.
        /// </summary>
        /// <param name="value">Stream length.</param>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when trying to change root directory matadata.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// <item>invalid aData size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when entrys aValue is null.</item>
        /// <item>Thrown when entrys aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error.</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error.</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aValue is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Thrown when aSize is smaller than 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when aName is null or empty string.</item>
        /// <item>aData length is 0.</item>
        /// <item>FAT type is unknown.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public override void SetLength(long value)
        {
            Global.Debugger.SendInternal("-- FatStream.SetLength --");
            Global.Debugger.SendInternal("value =");
            Global.Debugger.SendInternal(value);
            Global.Debugger.SendInternal("mFatTable.Length =");
            Global.Debugger.SendInternal(mFatTable.Length);

            mDirectoryEntry.SetSize(value);
            mSize = value;
            mFatTable = mDirectoryEntry.GetFatTable();
        }

        /// <summary>
        /// Read data from stream.
        /// </summary>
        /// <param name="aBuffer">A destination buffer.</param>
        /// <param name="aOffset">A offset in the buffer.</param>
        /// <param name="aCount">Number of bytes to read.</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if aCount or aOffset is smaller than 0 or bigger than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown on invalid offset length.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when aBuffer is null / memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error.</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error.</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
        public override int Read(byte[] aBuffer, int aOffset, int aCount)
        {
            Global.Debugger.SendInternal("-- FatStream.Read --");
            Global.Debugger.SendInternal("aBuffer.Length = " + aBuffer.Length);
            Global.Debugger.SendInternal("aOffset = " + aOffset);
            Global.Debugger.SendInternal("aCount = " + aCount);
            Global.Debugger.SendInternal("Current State");
            Global.Debugger.SendInternal("mPosition = " + mPosition);
            Global.Debugger.SendInternal("mDirectoryEntry.mSize = " + mDirectoryEntry.mSize);
            Global.Debugger.SendInternal("xClusterSize = " + mFS.BytesPerCluster);
            Global.Debugger.SendInternal("mFatTable = ");
            for (int i = 0; i < mFatTable.Length; i++)
            {
                Global.Debugger.SendInternal(mFatTable[i]);
            }

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
            byte[] xCluster = mFS.NewBlockArray();

            while (xCount > 0)
            {
                long xClusterIdx = mPosition / xClusterSize;
                long xPosInCluster = mPosition % xClusterSize;
                mFS.Read(mFatTable[(int)xClusterIdx], ref xCluster);
                long xReadSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xReadSize = xClusterSize - xPosInCluster; // -1
                }
                else
                {
                    xReadSize = xCount;
                }

                Global.Debugger.SendInternal("xClusterIdx = " + xClusterIdx);
                Global.Debugger.SendInternal("xPosInCluster = " + xPosInCluster);
                Global.Debugger.SendInternal("xReadSize = " + xReadSize);

                Array.Copy(xCluster, xPosInCluster, aBuffer, xOffset, xReadSize);

                xOffset += xReadSize;
                xCount -= xReadSize;
                mPosition += xReadSize;
            }

            Global.Debugger.SendInternal("aBuffer =" + BitConverter.ToString(aBuffer));
            Global.Debugger.SendInternal("xOffset =" + xOffset);

            return (int)xOffset;
        }

        /// <summary>
        /// Write to stream.
        /// </summary>
        /// <param name="aBuffer">A source buffer.</param>
        /// <param name="aOffset">A offset in the buffer.</param>
        /// <param name="aCount">Number of bytes to read.</param>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when trying to change root directory matadata.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// <item>invalid aData size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when entrys aValue is null.</item>
        /// <item>Thrown when entrys aData is null.</item>
        /// <item>Thrown when aBuffer is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error.</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error.</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aValue is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown if aCount or aOffset is smaller than 0 or bigger than Int32.MaxValue.</item>
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Thrown when aSize is smaller than 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown on invalid offset length.</item>
        /// <item>Thrown when aName is null or empty string.</item>
        /// <item>aData length is 0.</item>
        /// <item>FAT type is unknown.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public override void Write(byte[] aBuffer, int aOffset, int aCount)
        {
            Global.Debugger.SendInternal("-- FatStream.Write --");
            Global.Debugger.SendInternal("aBuffer.Length =");
            Global.Debugger.SendInternal(aBuffer.Length);
            Global.Debugger.SendInternal("aOffset =");
            Global.Debugger.SendInternal(aOffset);
            Global.Debugger.SendInternal("aCount =");
            Global.Debugger.SendInternal(aCount);

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

            long xTotalLength = mPosition + xCount;

            if (xTotalLength > Length)
            {
                SetLength(xTotalLength);
            }

            byte[] xCluster = mFS.NewBlockArray();
            while (xCount > 0)
            {
                long xWriteSize;
                long xClusterIdx = mPosition / xClusterSize;
                long xPosInCluster = mPosition % xClusterSize;
                if (xPosInCluster + xCount > xClusterSize)
                {
                    xWriteSize = xClusterSize - xPosInCluster;
                }
                else
                {
                    xWriteSize = xCount;
                }

                mFS.Read(mFatTable[xClusterIdx], ref xCluster);
                Array.Copy(aBuffer, aOffset, xCluster, (int)xPosInCluster, (int)xWriteSize);
                mFS.Write(mFatTable[xClusterIdx], xCluster);

                Global.Debugger.SendInternal("xClusterIdx =");
                Global.Debugger.SendInternal(xClusterIdx);
                Global.Debugger.SendInternal("xPosInCluster =");
                Global.Debugger.SendInternal(xPosInCluster);
                Global.Debugger.SendInternal("xWriteSize =");
                Global.Debugger.SendInternal(xWriteSize);

                xOffset += xWriteSize;
                xCount -= xWriteSize;
                aOffset += (int)xWriteSize;
                mPosition += xWriteSize;
            }

            GCImplementation.Free(xCluster);
        }
    }
}
