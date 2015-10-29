using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kudzu.BreakpointsKernel {
  public class NullableTest : Test {
    
    protected UInt32? mSize;
    public virtual UInt32? Size {
      get { return mSize; }
    }

    public override void Run() {
      mSize = 7;
      bool xHasValue = mSize.HasValue; 

      Chk(xHasValue);
      UInt32 x = mSize.Value;
      Chk(mSize.Value == 7); 

      UInt32? xSize;
      xSize = 4;

      UInt32? y = x;
      Chk(y.HasValue);
      Chk(y.Value == 7);

      UInt32 x2 = 64;
      UInt32? y2 = x2;
      Chk(y2.Value == 64);
      Chk(y.Value == 7);

      UInt32? y3 = x2;
      Chk(y3.Value == 64);
    }

  }
}
