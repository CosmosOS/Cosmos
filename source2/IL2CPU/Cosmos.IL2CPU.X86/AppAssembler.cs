using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;


namespace Cosmos.IL2CPU.X86 {
  public abstract class AppAssembler : IL2CPU.AppAssembler {
    public const string EndOfMethodLabelNameNormal = ".END__OF__METHOD_NORMAL";
    public const string EndOfMethodLabelNameException = ".END__OF__METHOD_EXCEPTION";


    protected AppAssembler(byte comportNumber)
      : base(new AssemblerNasm(comportNumber)) {
    }

    protected override void Move(string aDestLabelName, int aValue) {
      new CPUx86.Mov {
        DestinationRef = CPU.ElementReference.New(aDestLabelName),
        DestinationIsIndirect = true,
        SourceValue = (uint)aValue
      };
    }

    protected override void Push(uint aValue) {
      new CPUx86.Push {
        DestinationValue = aValue
      };
    }

    protected override void Pop() {
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mAssembler.Stack.Pop().Size };
    }

    protected override void Push(string aLabelName, bool isIndirect = false) {
      new CPUx86.Push {
        DestinationRef = CPU.ElementReference.New(aLabelName),
        DestinationIsIndirect = isIndirect
      };
    }

    protected override void Call(MethodBase aMethod) {
      new Cosmos.Assembler.x86.Call {
        DestinationLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName(aMethod)
      };
    }

    protected override void Jump(string aLabelName) {
      new Cosmos.Assembler.x86.Jump {
        DestinationLabel = aLabelName
      };
    }

    protected override int GetVTableEntrySize() {
      return 16; // todo: retrieve from actual type info
    }

    private const string InitStringIDsLabel = "___INIT__STRINGS_TYPE_ID_S___";


    public override void EmitEntrypoint(MethodBase aEntrypoint, IEnumerable<MethodBase> aMethods) {
      #region Literal strings fixup code
      // at the time the datamembers for literal strings are created, the type id for string is not yet determined. 
      // for now, we fix this at runtime.
      new Label(InitStringIDsLabel);
      new Push { DestinationReg = Registers.EBP };
      new Mov { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      new Mov { DestinationReg = Registers.EAX, SourceRef = Cosmos.Assembler.ElementReference.New(ILOp.GetTypeIDLabel(typeof(String))), SourceIsIndirect = true };
      foreach (var xDataMember in mAssembler.DataMembers) {
        if (!xDataMember.Name.StartsWith("StringLiteral")) {
          continue;
        }
        if (xDataMember.Name.EndsWith("__Contents")) {
          continue;
        }
        new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New(xDataMember.Name), DestinationIsIndirect = true, SourceReg = Registers.EAX };
      }
      new Pop { DestinationReg = Registers.EBP };
      new Return();
      #endregion
      new Label(CosmosAssembler.EntryPointName);
      new Push { DestinationReg = Registers.EBP };
      new Mov { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      new Call { DestinationLabel = InitVMTCodeLabel };
      new Call { DestinationLabel = InitStringIDsLabel };

      // we now need to do "newobj" on the entry point, and after that, call .Start on it
      var xCurLabel = CosmosAssembler.EntryPointName + ".CreateEntrypoint";
      new Label(xCurLabel);
      IL.Newobj.Assemble(Cosmos.Assembler.Assembler.CurrentInstance, null, null, xCurLabel, aEntrypoint.DeclaringType, aEntrypoint);
      xCurLabel = CosmosAssembler.EntryPointName + ".CallStart";
      new Label(xCurLabel);
      IL.Call.DoExecute(mAssembler, null, aEntrypoint.DeclaringType.BaseType.GetMethod("Start"), null, xCurLabel, CosmosAssembler.EntryPointName + ".AfterStart");
      new Label(CosmosAssembler.EntryPointName + ".AfterStart");
      new Pop { DestinationReg = Registers.EBP };
      new Return();


      if (ShouldOptimize)
      {
          Orvid.Optimizer.Optimize(Assembler);
      }
    }

    protected override void Ldarg(MethodInfo aMethod, int aIndex) {
      IL.Ldarg.DoExecute(mAssembler, aMethod, (ushort)aIndex);
    }

    protected override void Call(MethodInfo aMethod, MethodInfo aTargetMethod) {
      var xSize = IL.Call.GetStackSizeToReservate(aTargetMethod.MethodBase);
      if (xSize > 0) {
        new CPUx86.Sub { DestinationReg = Registers.ESP, SourceValue = xSize };
      }
      new CPUx86.Call { DestinationLabel = ILOp.GetMethodLabel(aTargetMethod) };
    }

    protected override void Ldflda(MethodInfo aMethod, string aFieldId) {
      IL.Ldflda.DoExecute(mAssembler, aMethod, aMethod.MethodBase.DeclaringType, aFieldId, false);
    }

    //// todo: remove when everything goes fine
    //protected override void AfterOp(MethodInfo aMethod, ILOpCode aOpCode) {
    //  base.AfterOp(aMethod, aOpCode);
    //  new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBP };
    //  var xTotalTransitionalStackSize = (from item in Stack
    //                                     select (int)ILOp.Align((uint)item.Size, 4)).Sum();
    //  // include locals too
    //  if (aMethod.MethodBase.DeclaringType.Name == "GCImplementationImpl"
    //    && aMethod.MethodBase.Name == "AllocNewObject") {
    //    Console.Write("");
    //  }
    //  var xLocalsValue = (from item in aMethod.MethodBase.GetMethodBody().LocalVariables
    //                      select (int)ILOp.Align(ILOp.SizeOfType(item.LocalType), 4)).Sum();
    //  var xExtraSize = xLocalsValue + xTotalTransitionalStackSize;

    //  new Sub { DestinationReg = Registers.EAX, SourceValue = (uint)xExtraSize };
    //  new Compare { DestinationReg = Registers.EAX, SourceReg = Registers.ESP };
    //  var xLabel = ILOp.GetLabel(aMethod, aOpCode) + "___TEMP__STACK_CHECK";
    //  new ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = xLabel };
    //  new Xchg { DestinationReg = Registers.BX, SourceReg = Registers.BX, Size = 16 };
    //  new Halt();

    //  new Label(xLabel);

    //}

    public override uint GetSizeOfType(Type aType) {
      return ILOp.SizeOfType(aType);
    }
  }
}