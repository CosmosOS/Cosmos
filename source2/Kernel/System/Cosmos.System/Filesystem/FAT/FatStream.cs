using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT {
  public class FatStream : Stream {
    protected readonly Listing.FatFile mFile;
    protected readonly FatFileSystem mFS = null;
    protected byte[] mReadBuffer;
    //TODO: In future we might read this in as needed rather than
    // all at once. This structure will also consume 2% of file size in RAM 
    // (for default cluster size of 2kb, ie 4 bytes per cluster)
    // so we might consider a way to flush it and only keep parts.
    // Example, a 100 MB file will require 2MB for this structure. That is
    // probably acceptable for the mid term future.
    protected List<UInt32> mFatTable;
    //TODO:UInt64
    protected UInt32? mReadBufferPosition;

    public FatStream(Listing.FatFile aFile) {
      mFile = aFile;
      mFS = mFile.FileSystem;
      mReadBuffer = mFile.FileSystem.NewClusterArray();

      if (mFile.Size > 0) {
        mFatTable = mFile.GetFatTable();
      }
    }

    public override bool CanSeek {
      get { return true; }
    }

    public override bool CanRead {
      get { return true; }
    }

    public override bool CanWrite {
      get { return false; }
    }

    public override long Length {
      get { return mFile.Size; }
    }

    protected UInt32 mPosition;
    public override long Position {
      get {
        return mPosition;
      }
      set {
        mPosition = (UInt32)value;
      }
    }

    public override int Read(byte[] aBuffer, int aOffset, int aCount) {
      if (aOffset < 0 || aCount < 0) {
        throw new ArgumentOutOfRangeException();
	  }
	  else if(aBuffer == null || aBuffer.Length - aOffset < aCount) {
        throw new ArgumentException("Invalid offset length!");
      } else if (mFile.FirstClusterNum == 0) {
        // FirstSector can be 0 for 0 length files
        return 0;
      } else if (mPosition == mFile.Size) {
        // EOF
        return 0;
      }

	  // reduce count, so that no out of bound exception occurs if not existing
	  // entry is used in line mFS.ReadCluster(mFatTable[(int)xClusterIdx], xCluster);
	  uint xMaxReadableBytes = mFile.Size - mPosition;
	  if (aCount > xMaxReadableBytes)
		  aCount = (int)xMaxReadableBytes;

      var xCluster = mFS.NewClusterArray();
      UInt32 xClusterSize = mFS.BytesPerCluster;

      while (aCount > 0) {
        UInt32 xClusterIdx = mPosition / xClusterSize;
        UInt32 xPosInCluster = mPosition % xClusterSize;
        mFS.ReadCluster(mFatTable[(int)xClusterIdx], xCluster);
        int xReadSize;
        if (xPosInCluster + aCount > xClusterSize) {
          xReadSize = (int)(xClusterSize - xPosInCluster - 1);
        } else {
          xReadSize = aCount;
        }
        // (int) casts are needed so we use the 32 bit version of the copy since the 64 bit arg
        // version is not supported currently.
        Array.Copy(xCluster, (int)xPosInCluster, aBuffer, (int)aOffset, (int)xReadSize);
        aOffset = aOffset + xReadSize;
        aCount = aCount - xReadSize;
      }

	  mPosition += (uint)aOffset;
      return (int)aOffset;
	}

    public override void Flush() {
      throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin) {
      throw new NotImplementedException();
    }

    public override void SetLength(long value) {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
      throw new NotImplementedException();
    }
  }
}