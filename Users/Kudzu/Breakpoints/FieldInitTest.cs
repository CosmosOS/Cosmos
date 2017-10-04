using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kudzu.BreakpointsKernel {
  public class FieldInitTest : Test {
    public override void Run() {
      FieldInitBase x = new FieldInit();
      Chk(x.mBaseInt == 512);
    }
  }

  public class FieldInitBase {
    public UInt64 mBaseInt;
  }

  public class FieldInit : FieldInitBase {
    public FieldInit() {
      mBaseInt = 512;
    }
  }
}
