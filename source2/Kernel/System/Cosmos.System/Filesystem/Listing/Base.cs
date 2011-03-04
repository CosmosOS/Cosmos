using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.Listing {
  public abstract class Base {
    public readonly FileSystem FileSystem;
    public readonly string Name;

    public Base(FileSystem aFileSystem, string aName) {
      FileSystem = aFileSystem;
      Name = aName;
    }

    // Size might be updated in an ancestor destructor or on demand,
    // so its not a readonly field
    //TODO:UInt64? Size
    protected UInt32 mSize;
    public virtual UInt32 Size {
      get { return mSize; }
    }

  }
}
