using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  public class Assembler : Cosmos.IL2CPU.Assembler {

    protected override void InitILOps() {
      var xILOp = new ILOp(this);
      // Don't change the type in the foreach to a var, its necessary as it is now
      // to typecast it, so we can then recast to an int.
      foreach (ILOpCode.Code xCode in Enum.GetValues(typeof(ILOpCode.Code))) {
        int xCodeValue = (int)xCode;
        if (xCodeValue <= 0xFF) {
          mILOpsLo[xCodeValue] = xILOp;
        } else {
          mILOpsHi[xCodeValue & 0xFF] = xILOp;
        }
      }
    }

    protected override void MethodBegin(MethodInfo aMethod) {
    }

    protected override void MethodEnd(MethodInfo aMethod) {
    }


    protected override void Push(uint aValue) {
      throw new NotImplementedException();
    }

    protected override void Push(string aLabelName) {
      throw new NotImplementedException();
    }

    protected override void Call(System.Reflection.MethodBase aMethod) {
      throw new NotImplementedException();
    }

    protected override void Move(string aDestLabelName, int aValue) {
      throw new NotImplementedException();
    }
  
    protected override int GetVTableEntrySize() {
      return 0;
    }
  
  }
}
