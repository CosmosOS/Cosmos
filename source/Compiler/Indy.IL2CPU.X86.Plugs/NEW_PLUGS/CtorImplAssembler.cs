using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86.IL;
using CPUx86 = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using NewAssembler = Cosmos.IL2CPU.Assembler;

namespace Indy.IL2CPU.X86.Plugs.NEW_PLUGS {
  public class CtorImplAssembler: AssemblerMethod {
    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
      throw new NotImplementedException();
    }

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      // method signature: $this, object @object, IntPtr method
      var xMethodInfo = (MethodInfo)aMethodInfo;
      var xAssembler = (NewAssembler)aAssembler;
      new Comment("Save target ($this) to field");
      new Comment("-- ldarg 0");
      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
      //Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
      new Comment("-- ldarg 1");
      Ldarg.DoExecute(xAssembler, xMethodInfo, 1);
      new Comment("-- stfld _target");
      Stfld.DoExecute(xAssembler, xMethodInfo, "System.Object System.Delegate._target", xMethodInfo.MethodBase.DeclaringType, true);
      new Comment("Save method pointer to field");
      //Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
      new Comment("-- ldarg 0");
      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
      new Comment("-- ldarg 2");
      Ldarg.DoExecute(xAssembler, xMethodInfo, 2);
      new Comment("-- stfld _methodPtr");
      Stfld.DoExecute(xAssembler, xMethodInfo, "System.IntPtr System.Delegate._methodPtr", xMethodInfo.MethodBase.DeclaringType, true);
      new Comment("Saving ArgSize to field");
      uint xSize = 0;
      foreach (var xArg in xMethodInfo.MethodBase.GetParameters().Skip(2)) {
        xSize += ILOp.Align(ILOp.SizeOfType(xArg.ParameterType), 4);
      }
      new Comment("-- ldarg 0");
      Ldarg.DoExecute(xAssembler, xMethodInfo, 0);
      new CPUx86.Push { DestinationValue = xSize };
      new Comment("-- push argsize");
      xAssembler.Stack.Push((int)ILOp.SizeOfType(typeof(int)), typeof(int));
      new Comment("-- stfld ArgSize");
      Stfld.DoExecute(xAssembler, xMethodInfo, "$$ArgSize$$", xMethodInfo.MethodBase.DeclaringType, true);


          //public static void Ctor(Delegate aThis, object aObject, IntPtr aMethod, 
      //[FieldAccess(Name = "System.Object System.Delegate._target")] ref object aFldTarget, 
      //[FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref IntPtr aFldMethod) {
    }
  }
}
