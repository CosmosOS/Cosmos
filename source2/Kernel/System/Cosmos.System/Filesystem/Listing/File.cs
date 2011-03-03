using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.Listing {
  public class File : Base {
    public File(FileSystem aFileSystem, string aName, UInt64? aSize) : base(aFileSystem, aName) {
      mSize = aSize;
    }
  }
}
