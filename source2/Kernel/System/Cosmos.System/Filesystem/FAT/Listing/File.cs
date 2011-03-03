using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT.Listing {
  public class File : Cosmos.System.Filesystem.Listing.File {
    public readonly FileSystemFAT FileSystem;
    public readonly UInt32 FirstCluster;

    // Size is UInt32 because FAT doesn't support bigger.
    // Dont change to UInt64
    public File(FileSystemFAT aFileSystem, string aName, UInt32 aSize, UInt32 aFirstCluster)
      : base(aFileSystem, aName, aSize) {
      FileSystem = aFileSystem;
      FirstCluster = aFirstCluster;
    }

  }
}
