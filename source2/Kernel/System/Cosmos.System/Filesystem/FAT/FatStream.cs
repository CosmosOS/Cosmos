using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT {
  public class FatStream : Stream {
    protected readonly Listing.File mFileListing;
    protected byte[] mBuffer;

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
      get { return mFileListing.Size.Value; }
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

    public override int Read(byte[] buffer, int offset, int count) {
      // FirstSector can be 0 for 0 length files
      return 0;
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

    public FatStream(Listing.File aFileListing) {
      mFileListing = aFileListing;
      mBuffer = aFileListing.FileSystem.NewClusterArray();
    }
  }
}
