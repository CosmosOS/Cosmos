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

  public override void Run() {
      UInt32 x = 32;
      UInt32? y = x;
      Chk(y.Value == 32);

      UInt32 x2 = 64;
      UInt32? y2 = x2;
      Chk(y2.Value == 64);
      Chk(y.Value == 32);

      UInt32? y3 = x2;
      Chk(y3.Value == 64);
    }

  }
}
