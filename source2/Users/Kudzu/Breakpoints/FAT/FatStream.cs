using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Kudzu.BreakpointsKernel.FAT
{
  public class MyFatStream : Stream
  {
    protected readonly Listing.MyFatFile mFile;
    protected readonly MyFatFileSystem mFS = null;
    protected byte[] mReadBuffer;
    //TODO: In future we might read this in as needed rather than
    // all at once. This structure will also consume 2% of file size in RAM 
    // (for default cluster size of 2kb, ie 4 bytes per cluster)
    // so we might consider a way to flush it and only keep parts.
    // Example, a 100 MB file will require 2MB for this structure. That is
    // probably acceptable for the mid term future.
    protected List<UInt64> mFatTable;
    protected UInt64? mReadBufferPosition;

    public MyFatStream(Listing.MyFatFile aFile)
    {
      mFile = aFile;
      mFS = mFile.FileSystem;
      mReadBuffer = mFile.FileSystem.NewClusterArray();
      var xSize = mFile.Size;
      if (xSize > 0)
      {
        mFatTable = mFile.GetFatTable();
      }
    }

    public override bool CanSeek
    {
      get { return true; }
    }

    public override bool CanRead
    {
      get { return true; }
    }

    public override bool CanWrite
    {
      get { return false; }
    }

    public override long Length
    {
      get { return (long)mFile.Size; }
    }

    protected UInt64 mPosition;
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
      return Read(aBuffer, (Int64)aOffset, (Int64)aCount);
    }

    public int Read(byte[] aBuffer, Int64 aOffset, Int64 aCount)
    {
      //ulong xCount = (ulong)aCount;
      
      var xCluster = mFS.NewClusterArray();
      

      //while (xCount > 0)
      {
        UInt64 xClusterIdx = 1;
        mFS.ReadCluster((ulong)mFatTable[(int)xClusterIdx], xCluster);
        //xCount = 0;
      }

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
      throw new NotImplementedException();
    }

    public override void Write(byte[] aBuffer, int aOffset, int aCount)
    {
      Write(aBuffer, (long)aOffset, (long)aCount);
    }

    public void Write(byte[] buffer, long offset, long count)
    {
      throw new NotImplementedException();
    }
  }
}