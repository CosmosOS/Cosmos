using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Debug.Consts;
using Cosmos.Assembler.XSharp;

namespace Cosmos.Debug.DebugStub {
  public partial class DebugStub : CodeGroup {
    public DebugStub(int aComNo) {
      mComNo = aComNo;
      mComAddr = mComPortAddresses[mComNo - 1];
      mComStatusAddr = (UInt16)(mComAddr + 5);

      // Old method, need to convert to fields
      mAsm.DataMembers.AddRange(new DataMember[]{
        // Breakpoint addresses
        //new DataMember("DebugBPs", new int[256]),
        //TODO: Move to DebugStub (new)
        new DataMember("DebugWaitMsg", "Waiting for debugger connection...")
      });
    }

  }
}
