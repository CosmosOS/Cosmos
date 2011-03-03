using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.Listing {
  public class File : Base {
    //TODO:UInt64 Size
    public File(FileSystem aFileSystem, string aName, UInt32? aSize)
      : base(aFileSystem, aName) {
      mSize = aSize;
    }
  }
}
