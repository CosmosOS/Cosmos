using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using System.Linq;
using XSharp.Compiler;
using SysReflection = System.Reflection;


namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldsfld)]
  public class Ldsfld : ILOp
  {
    public Ldsfld(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {

      var xType = aMethod.MethodBase.DeclaringType;
      var xOpCode = (OpField)aOpCode;
      SysReflection.FieldInfo xField = xOpCode.Value;

      // call cctor:
      var xCctor = (xField.DeclaringType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic)).SingleOrDefault();
      if (xCctor != null)
      {
        XS.Call(LabelName.Get(xCctor));
        ILOp.EmitExceptionLogic(Assembler, aMethod, aOpCode, true, null, ".AfterCCTorExceptionCheck");
        XS.Label(".AfterCCTorExceptionCheck");
      }

      //Assembler.Stack.Pop();
      //int aExtraOffset;// = 0;
      //bool xNeedsGC = xField.FieldType.IsClass && !xField.FieldType.IsValueType;
      var xSize = SizeOfType(xField.FieldType);
      //if( xNeedsGC )
      //{
      //    aExtraOffset = 12;
      //}

      string xDataName = DataMember.GetStaticFieldName(xField);

      var xTypeNeedsGC = TypeIsReferenceType(xField.FieldType);
      if (xTypeNeedsGC)
      {
        XS.Push(xDataName, isIndirect: true, displacement: 4);
        XS.Push(0);
        return;
      }


      if (xSize >= 4)
      {
        for (int i = 1; i <= (xSize / 4); i++)
        {
          //	Pop("eax");
          //	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
          new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New(xDataName), DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize - (i * 4)) };
        }
        switch (xSize % 4)
        {
          case 1:
            {
              XS.Set(XSRegisters.EAX, 0);
              XS.Set(XSRegisters.AL, xDataName, sourceIsIndirect: true);
              XS.Push(XSRegisters.EAX);
              break;
            }
          case 2:
            {
              XS.Set(XSRegisters.EAX, 0);
              XS.Set(XSRegisters.AX, xDataName, sourceIsIndirect: true);
              XS.Push(XSRegisters.EAX);
              break;
            }
          case 0:
            {
              break;
            }
          default:
            //EmitNotImplementedException( Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + ( xSize % 4 ) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
            throw new NotImplementedException();
            //break;
        }
      }
      else
      {
        switch (xSize)
        {
          case 1:
            {
              XS.Set(XSRegisters.EAX, 0);
              XS.Set(XSRegisters.AL, xDataName, sourceIsIndirect: true);
              XS.Push(XSRegisters.EAX);
              break;
            }
          case 2:
            {
              XS.Set(XSRegisters.EAX, 0);
              XS.Set(XSRegisters.AX, xDataName, sourceIsIndirect: true);
              XS.Push(XSRegisters.EAX);
              break;
            }
          case 0:
            {
              break;
            }
          default:
            //EmitNotImplementedException( Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + ( xSize % 4 ) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
            throw new NotImplementedException();
            //break;
        }
      }
    }
  }
}
