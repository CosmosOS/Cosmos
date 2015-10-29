using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kudzu.BreakpointsKernel {
  public class Int64Test : Test {

    public override void Run() {
      UInt64 x = 0xFFFFFFFFFFFFFFFF;
      
      x = x - 1;
      Chk(x == 0xFFFFFFFFFFFFFFFE);

      x = x + 1;
      Chk(x == 0xFFFFFFFFFFFFFFFF);

      x = 0x5555555555555555;
      x = x * 2;
      Chk(x == 0xAAAAAAAAAAAAAAAA);

      x = 0xAAAAAAAAAAAAAAAA;
      x = x / 2; // This line dies with CPU exception 0x00
      Chk(x == 0x5555555555555555);

      // move to uint32 and back
      // add with uint32 etc
    }

  }
}
