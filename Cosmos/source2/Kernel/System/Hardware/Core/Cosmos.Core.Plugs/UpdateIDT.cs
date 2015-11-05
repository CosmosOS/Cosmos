using System;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using CPUAll = Cosmos.Assembler;
using System.Collections.Generic;

namespace Cosmos.Core.Plugs.Assemblers {
  //TODO: This asm refs Hardware.. should not.. its a higher ring
  public class UpdateIDT : AssemblerMethod {
    private static MethodBase GetMethodDef(Assembly aAssembly, string aType, string aMethodName, bool aErrorWhenNotFound) {
      System.Type xType = aAssembly.GetType(aType, false);
      if (xType != null) {
        MethodBase xMethod = xType.GetMethod(aMethodName);
        if (xMethod != null) {
          return xMethod;
        }
      }
      if (aErrorWhenNotFound) {
        throw new System.Exception("Method '" + aType + "::" + aMethodName + "' not found!");
      }
      return null;
    }

    private static MethodBase GetInterruptHandler(byte aInterrupt) {
      return GetMethodDef(typeof(Cosmos.Core.INTs).Assembly, typeof(Cosmos.Core.INTs).FullName
        , "HandleInterrupt_" + aInterrupt.ToString("X2"), false);
    }

    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      // IDT is already initialized but just for base hooks, and asm only.
      // ie Int 1, 3 and GPF
      // This routine updates the IDT now that we have C# running to allow C# hooks to handle
      // the other INTs

      // We are updating the IDT, disable interrupts
      new CPUx86.ClrInterruptFlag();

      for (int i = 0; i < 256; i++) {
        // These are already mapped, don't remap them.
        // Maybe in the future we can look at ones that are present
        // and skip them, but some we may want to overwrite anyways.
        if (i == 1 || i == 3) {
          continue;
        }

        new CPUx86.Mov {DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("__ISR_Handler_" + i.ToString("X2")) };
        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true, DestinationDisplacement = ((i * 8) + 0),
          SourceReg = CPUx86.Registers.AL
        };
        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true, DestinationDisplacement = ((i * 8) + 1),
          SourceReg = CPUx86.Registers.AH
        };
        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true, DestinationDisplacement = ((i * 8) + 2),
          SourceValue = 0x8,
          Size = 8
        };

        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true,
          DestinationDisplacement = ((i * 8) + 5),
          SourceValue = 0x8E,
          Size = 8
        };
        new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 16 };
        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true,
          DestinationDisplacement = ((i * 8) + 6),
          SourceReg = CPUx86.Registers.AL
        };
        new CPUx86.Mov {
          DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
          DestinationIsIndirect = true,
          DestinationDisplacement = ((i * 8) + 7),
          SourceReg = CPUx86.Registers.AH
        };
      }

      new CPUx86.Jump { DestinationLabel = "__AFTER__ALL__ISR__HANDLER__STUBS__" };
      var xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
      for (int j = 0; j < 256; j++) {
        new CPUAll.Label("__ISR_Handler_" + j.ToString("X2"));
        new CPUx86.Call { DestinationLabel = "__INTERRUPT_OCCURRED__" };

        if (Array.IndexOf(xInterruptsWithParam, j) == -1) {
          new CPUx86.Push { DestinationValue = 0 };
        }
        new CPUx86.Push { DestinationValue = (uint)j };
        new CPUx86.Pushad();

        new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP }; // preserve old stack address for passing to interrupt handler

        // store floating point data
        new CPUx86.And { DestinationReg = CPUx86.Registers.ESP, SourceValue = 0xfffffff0 }; // fxsave needs to be 16-byte alligned
        new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 512 }; // fxsave needs 512 bytes
        new CPUx86.x87.FXSave { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true }; // save the registers
        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ESP };

        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX }; // 
        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX }; // pass old stack address (pointer to InterruptContext struct) to the interrupt handler
        //new CPUx86.Move("eax",
        //                "esp");
        //new CPUx86.Push("eax");
        new CPUx86.JumpToSegment { Segment = 8, DestinationLabel = "__ISR_Handler_" + j.ToString("X2") + "_SetCS" };
        new CPUAll.Label("__ISR_Handler_" + j.ToString("X2") + "_SetCS");
        MethodBase xHandler = GetInterruptHandler((byte)j);
        if (xHandler == null) {
          xHandler = GetMethodDef(typeof(INTs).Assembly, typeof(INTs).FullName, "HandleInterrupt_Default", true);
        }
        new CPUx86.Call { DestinationLabel = CPUAll.LabelName.Get(xHandler) };
        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        new CPUx86.x87.FXStore { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };

        new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX }; // this restores the stack for the FX stuff, except the pointer to the FX data
        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 }; // "pop" the pointer

        new CPUx86.Popad();

        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
        new CPUAll.Label("__ISR_Handler_" + j.ToString("X2") + "_END");
        new CPUx86.IRET();
      }
      new CPUAll.Label("__INTERRUPT_OCCURRED__");
      new CPUx86.Return();
      new CPUAll.Label("__AFTER__ALL__ISR__HANDLER__STUBS__");
      new CPUx86.Noop();
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = ".__AFTER_ENABLE_INTERRUPTS" };

      // Reenable interrupts
      new CPUx86.Sti();
      
      new CPUAll.Label(".__AFTER_ENABLE_INTERRUPTS");
    }
  }
}
