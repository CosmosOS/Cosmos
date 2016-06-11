using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmos.Core.Memory.Test {
  [TestClass]
  public class UnitTest1 {
    [TestMethod]
    unsafe public void TestMethod1() {
      var xRAM = new byte[128 * 1024 * 1024];
      xRAM[0] = 1;
      fixed (byte* xPtr = xRAM) {
        CHeap.Init(xPtr, (UInt32)xRAM.LongLength);
      }
    }
  }
}
