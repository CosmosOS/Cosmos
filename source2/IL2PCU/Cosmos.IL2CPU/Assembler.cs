using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.IL2CPU {
  public abstract class Assembler {

    protected ILOp[] mILOpsLo = new ILOp[256];
    protected ILOp[] mILOpsHi = new ILOp[256];

    public Assembler() {
      InitILOps();
    }

    public void ProcessMethod(UInt32 aMethodUID, List<ILOpCode> aOpCodes) {
      foreach (var xOpCode in aOpCodes) {
        ILOp xILOp;
        uint xOpCodeVal = (uint)xOpCode.OpCode;
        if (xOpCodeVal <= 0xFF) {
          xILOp = mILOpsLo[xOpCodeVal];
        } else {
          xILOp = mILOpsHi[xOpCodeVal & 0xFF];
        }
        xILOp.Execute(aMethodUID, xOpCode);
      }
    }

    protected abstract void InitILOps();

    protected void InitILOps(Type aAssemblerBaseOp) {
      foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aAssemblerBaseOp)) {
          var xAttribs = (OpCodeAttribute[])xType.GetCustomAttributes(typeof(OpCodeAttribute), false);
          foreach(var xAttrib in xAttribs) {
            var xOpCode = (ushort)xAttrib.OpCode;
            var xCtor = xType.GetConstructor(new Type[] {typeof(Assembler)});
            var xILOp = (ILOp)xCtor.Invoke(new Object[] {this});
            if (xOpCode <= 0xFF) {
              mILOpsLo[xOpCode] = xILOp;
            } else {
              mILOpsHi[xOpCode & 0xFF] = xILOp;
            }
          }
        }
      }
    }
  }
}