using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.Listing {
  public abstract class Base {

    protected string mName;
    public virtual string Name {
      get { return mName; }
      set { mName = value; }
    }

  }
}
