using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.IL2CPU {
  public abstract class Assembler {

    protected delegate ILOp ILOpCreateDelegate(ILOpCode aOpCode);
    protected ILOpCreateDelegate[] mILOpsLo = new ILOpCreateDelegate[256];
    protected ILOpCreateDelegate[] mILOpsHi = new ILOpCreateDelegate[256];

    public Assembler() {
      InitILOps();
    }

    public void ProcessMethod(UInt32 aMethodUID, List<ILOpCode> aOpCodes) {
      foreach (var xOpCode in aOpCodes) {
        ILOpCreateDelegate xCtor;
        uint xOpCodeVal = (uint)xOpCode.OpCode;
        if (xOpCodeVal <= 0xFF) {
          xCtor = mILOpsLo[xOpCodeVal];
        } else {
          xCtor = mILOpsHi[xOpCodeVal & 0xFF];
        }
				var xILOp = xCtor(xOpCode);
        xILOp.Execute(aMethodUID);
      }
    }

    // http://blogs.msdn.com/haibo_luo/archive/2005/11/17/494009.aspx
    // TODO: The ops are very minimalistic, we can probably do better by converting them to instances
    // and passing all data across execute. They have no instance data that needs preserved between calls, etc.
    protected ILOpCreateDelegate CreateCtorDelegate(Type aType) {
      var xMethod = new DynamicMethod("", typeof(ILOp), new Type[] { typeof(ILOpCode) }, typeof(ILScanner).Module);
      var xGen = xMethod.GetILGenerator();
      xGen.Emit(OpCodes.Ldarg_0);
      xGen.Emit(OpCodes.Newobj, aType.GetConstructor(new Type[] { typeof(ILOpCode) }));
      xGen.Emit(OpCodes.Ret);
      return (ILOpCreateDelegate)xMethod.CreateDelegate(typeof(ILOpCreateDelegate));
    }

    protected abstract void InitILOps();

    protected void InitILOps(Type aAssemblerBaseOp) {
      foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aAssemblerBaseOp)) {
          var xAttrib = (OpCodeAttribute)xType.GetCustomAttributes(typeof(OpCodeAttribute), false)[0];
          var xOpCode = (ushort)xAttrib.OpCode;
          var xDelegate = CreateCtorDelegate(xType);
          if (xOpCode <= 0xFF) {
            mILOpsLo[xOpCode] = xDelegate;
          } else {
            mILOpsHi[xOpCode & 0xFF] = xDelegate;
          }
        }
      }
    }
  }
}