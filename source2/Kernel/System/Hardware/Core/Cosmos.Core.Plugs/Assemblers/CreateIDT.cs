using System;
using System.Reflection;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPUAll = Cosmos.Compiler.Assembler;
using HW = Cosmos.Hardware2;
using System.Collections.Generic;

namespace Cosmos.Core.Plugs.Assemblers {
    //TODO: This asm refs Hardware.. should not.. its a higher ring
    public class CreateIDT : AssemblerMethod {
        private static MethodBase GetMethodDef(Assembly aAssembly,
                                               string aType,
                                               string aMethodName,
                                               bool aErrorWhenNotFound) {
            System.Type xType = aAssembly.GetType(aType,
                                                  false);
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
            return GetMethodDef(typeof(Cosmos.Hardware2.Interrupts).Assembly,
                                typeof(Cosmos.Hardware2.Interrupts).FullName,
                                "HandleInterrupt_" + aInterrupt.ToString("X2"),
                                false);
        }

        // there's one argument. 
        public override void AssembleNew(object aAssembler, object aMethodInfo) {
            #region generate IDT table

            string xFieldName = "_NATIVE_IDT_Contents";
            var xAssembler = (Assembler)aAssembler;
            xAssembler.DataMembers.Add(new CPUAll.DataMember(xFieldName,
                                                      new byte[8 * 256]));
            for (int i = 0; i < 256; i++) {
                new CPUx86.Move {
                    DestinationReg = CPUx86.Registers.EAX,
                    SourceRef = CPUAll.ElementReference.New("__ISR_Handler_" + i.ToString("X2"))
                };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 0),
                    SourceReg = CPUx86.Registers.AL
                };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 1),
                    SourceReg = CPUx86.Registers.AH
                };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 2),
                    SourceValue = 0x8,
                    Size = 8
                };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 5),
                    SourceValue = 0x8E,
                    Size = 8
                };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 16 };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 6),
                    SourceReg = CPUx86.Registers.AL
                };
                new CPUx86.Move {
                    DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents"),
                    DestinationIsIndirect = true,
                    DestinationDisplacement = ((i * 8) + 7),
                    SourceReg = CPUx86.Registers.AH
                };
            }
            new CPUAll.Label("______AFTER__IDT__TABLE__INIT__");
            xFieldName = "_NATIVE_IDT_Pointer";
            xAssembler.DataMembers.Add(new CPUAll.DataMember(xFieldName,
                                                      new ushort[] { 0x7FF, 0, 0 }));
            new CPUx86.Move {
                DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer"),
                DestinationIsIndirect = true,
                DestinationDisplacement = 2,
                SourceRef = CPUAll.ElementReference.New("_NATIVE_IDT_Contents")
            };

            #endregion
            new CPUx86.Move {
                DestinationReg = CPUx86.Registers.EAX,
                SourceRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer")
            };
            new CPUAll.Label(".RegisterIDT");
            new CPUx86.Lidt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Jump { DestinationLabel = "__AFTER__ALL__ISR__HANDLER__STUBS__" };
            var xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
            for (int j = 0; j < 256; j++) {
                new CPUAll.Label("__ISR_Handler_" + j.ToString("X2"));
                new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("InterruptsEnabledFlag"), DestinationIsIndirect = true, SourceValue = 0, Size = 32 };
                if (Array.IndexOf(xInterruptsWithParam,
                                  j) ==
                    -1) {
                    new CPUx86.Push { DestinationValue = 0 };
                }
                new CPUx86.Push { DestinationValue = (uint)j };
                new CPUx86.Pushad();

                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP }; // preserve old stack address for passing to interrupt handler

                // store floating point data
                new CPUx86.And { DestinationReg = CPUx86.Registers.ESP, SourceValue = 0xfffffff0 }; // fxsave needs to be 16-byte alligned
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 512 }; // fxsave needs 512 bytes
                new CPUx86.x87.FXSave { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true }; // save the registers
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ESP };

                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX }; // 
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX }; // pass old stack address (pointer to InterruptContext struct) to the interrupt handler
                //new CPUx86.Move("eax",
                //                "esp");
                //new CPUx86.Push("eax");
                new CPUx86.JumpToSegment { Segment = 8, DestinationLabel = "__ISR_Handler_" + j.ToString("X2") + "_SetCS" };
                new CPUAll.Label("__ISR_Handler_" + j.ToString("X2") + "_SetCS");
                MethodBase xHandler = GetInterruptHandler((byte)j);
                if (xHandler == null) {
                    xHandler = GetMethodDef(typeof(Cosmos.Hardware2.Interrupts).Assembly,
                                            typeof(Cosmos.Hardware2.Interrupts).FullName,
                                            "HandleInterrupt_Default",
                                            true);
                }
                new CPUx86.Call { DestinationLabel = CPUAll.MethodInfoLabelGenerator.GenerateLabelName(xHandler) };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.x87.FXStore { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };

                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX }; // this restores the stack for the FX stuff, except the pointer to the FX data
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 }; // "pop" the pointer

                new CPUx86.Popad();

                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                new CPUAll.Label("__ISR_Handler_" + j.ToString("X2") + "_END");
                // MtW: Appearantly, we dont need to enable interrupts on exit
                //if (j < 0x20 || j > 0x2F) {
                //new CPUx86.Sti();
                new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("InterruptsEnabledFlag"), DestinationIsIndirect = true, SourceValue = 1, Size = 32 };
                //} 
                new CPUx86.InterruptReturn();
            }
            new CPUAll.Label("__AFTER__ALL__ISR__HANDLER__STUBS__");
            new CPUx86.Noop();
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = ".__AFTER_ENABLE_INTERRUPTS" };
            new CPUx86.Sti();
            new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("InterruptsEnabledFlag"), DestinationIsIndirect = true, SourceValue = 1, Size = 32 };
            new CPUAll.Label(".__AFTER_ENABLE_INTERRUPTS");
        }
    }
}
