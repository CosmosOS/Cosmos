using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kudzu.BreakpointsKernel {
  public abstract class Test {
    
    public abstract void Run();

    public void Chk(bool aTest) {
      if (!aTest) {
        throw new Exception("Test failed.");
      }
    }

  }
}
