using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT.Listing {
  public class Directory : Cosmos.System.Filesystem.Listing.Directory {
    public Directory(FileSystem aFileSystem, string aName)
      : base(aFileSystem, aName) {
    }
  }
}
