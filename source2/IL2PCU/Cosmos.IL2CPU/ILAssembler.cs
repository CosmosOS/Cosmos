using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.IL2CPU {
  public class ILAssembler {

    protected delegate ILOp ILOpCreateDelegate(ILOpCode aOpCode);
    protected ILOpCreateDelegate[] mILOpsLo = new ILOpCreateDelegate[256];
    protected ILOpCreateDelegate[] mILOpsHi = new ILOpCreateDelegate[256];

    public ILAssembler(Type aAssemblerBaseOp) : this(aAssemblerBaseOp, false) {
    }

    public ILAssembler(Type aAssemblerBaseOp, bool aSingleILOp) {
      if (aSingleILOp) {
        LoadILOp(aAssemblerBaseOp);
      } else {
        LoadILOps(aAssemblerBaseOp);
      }
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

    protected ILOpCreateDelegate CreateCtorDelegate(Type aType) {
      var xMethod = new DynamicMethod("", typeof(ILOp), new Type[] { typeof(ILOpCode) }, typeof(ILScanner).Module);
      var xGen = xMethod.GetILGenerator();
      xGen.Emit(OpCodes.Ldarg_1);
      xGen.Emit(OpCodes.Newobj, aType.GetConstructor(new Type[] { typeof(ILOpCode) }));
      xGen.Emit(OpCodes.Ret);
      return (ILOpCreateDelegate)xMethod.CreateDelegate(typeof(ILOpCreateDelegate));
    }

    protected void LoadILOp(Type aAssemblerBaseOp) {
      // http://blogs.msdn.com/haibo_luo/archive/2005/11/17/494009.aspx
      //
      var xDelegate = CreateCtorDelegate(aAssemblerBaseOp);
      // Don't change the type in the foreach to a var, its necessary as it is now
      // to typecast it, so we can then recast to an int.
      foreach (ILOpCode.Code xCode in Enum.GetValues(typeof(ILOpCode.Code))) {
        int xCodeValue = (int)xCode;
        if (xCodeValue <= 0xFF) {
          mILOpsLo[xCodeValue] = xDelegate;
        } else {
          mILOpsHi[xCodeValue & 0xFF] = xDelegate;
        }
      }
    }

    protected void LoadILOps(Type aAssemblerBaseOp) {
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
