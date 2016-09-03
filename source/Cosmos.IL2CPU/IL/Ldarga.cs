using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler.x86;
using XSharp.Compiler;
using SysReflection = System.Reflection;


namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarga)]
  public class Ldarga : ILOp
  {
    public Ldarga(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      DoExecute(Assembler, aMethod, xOpVar.Value);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, ushort aParam)
    {
      var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, aParam);
      var xType = Ldarg.GetArgumentType(aMethod, aParam);

      XS.Comment("Arg idx = " + aParam);
      XS.Comment("Arg type = " + xType);

      XS.Set(XSRegisters.EAX, XSRegisters.EBP);
      XS.Set(XSRegisters.EBX, (uint)(xDisplacement));
      XS.Add(XSRegisters.EAX, XSRegisters.EBX);
      XS.Push(XSRegisters.EAX);
    }
  }
}
