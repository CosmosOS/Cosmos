using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BreakpointsKernel {
  public class NullableTest : Test {
    
  // Changeset 74104 - BreakpointsOS.cs. TestNullableTypes. 
  // It appears to work, but whatever value is last used there shows up for the Size value later on in line 123. 
  // If you comment out the x2 and y2 you will see 32 instead.
  // How to reproduce this outside of this changeset? Cant seem to repro it here.

    protected UInt32? mSize;
    public virtual UInt32? Size {
      get { return mSize; }
    }
    
    public override void Run() {
      UInt32 x = 32;
      UInt32? y = x;
      Chk(y.HasValue);
      Chk(y.Value == 32);

      UInt32 x2 = 64;
      UInt32? y2 = x2;
      Chk(y2.Value == 64);
      Chk(y.Value == 32);

      UInt32? y3 = x2;
      Chk(y3.Value == 64);

      mSize = 7;
      bool xHasValue = mSize.HasValue; // .HasValue is false
      Chk(xHasValue);
      x = mSize.Value;
      if (mSize.Value == 7) {
        int i = 5;
      }
      Chk(mSize.Value == 7); // Dies with 0x05 here
    }

  }
}
