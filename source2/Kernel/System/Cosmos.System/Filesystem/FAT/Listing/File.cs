using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT.Listing {
  public class File : Cosmos.System.Filesystem.Listing.File {
    protected readonly UInt32 mFirstCluster;

    public File(FileSystem aFileSystem, string aName, UInt64 aSize, UInt32 aFirstCluster)
      : base(aFileSystem, aName, aSize) {
      mFirstCluster = aFirstCluster;
    }

    // This is a temp POC function
    public byte[] FileContents() {
      var xResult = new byte[Size.Value];
      
      return xResult;
    }

  }
}
