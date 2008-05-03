using System;
using System.Reflection;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using HW = Cosmos.Hardware;
using System.Collections.Generic;

namespace Cosmos.Kernel.Plugs.Assemblers {
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
            return GetMethodDef(typeof(Cosmos.Hardware.PC.Interrupts).Assembly,
                                typeof(Cosmos.Hardware.PC.Interrupts).FullName,
                                "HandleInterrupt_" + aInterrupt.ToString("X2"),
                                false);
        }

        public override void Assemble(Assembler aAssembler) {
            #region generate IDT table

            string xFieldData = "";
            string xFieldName = "_NATIVE_IDT_Contents";
            for (int i = 0; i < 256; i++) {
                //xFieldData += "(__ISR_Handler_" + i.ToString("X2") + " and 0xFF),";
                //xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 8) and 0xFF),";
                //xFieldData += "0x8,0,";
                //xFieldData += "0,";
                //xFieldData += "0x8E,";
                //xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 16) and 0xFF),";
                //xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 24) and 0xFF),";
                xFieldData += "0,0,0,0,0,0,0,0,";
            }
            aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(aAssembler.CurrentGroup,
                                                                            new DataMember(xFieldName,
                                                                                           "db",
                                                                                           xFieldData.TrimEnd(','))));
            for (int i = 0; i < 256; i++) {
                new CPUx86.Move(Registers.EAX,
                                "__ISR_Handler_" + i.ToString("X2"));
                new CPUx86.Move("[_NATIVE_IDT_Contents + " + ((i*8) + 0) + "]",
                                Registers.AL);
                new CPUx86.Move("[_NATIVE_IDT_Contents + " + ((i*8) + 1) + "]",
                                Registers.AH);
                new CPUx86.Move("byte [_NATIVE_IDT_Contents + " + ((i*8) + 2) + "]",
                                "0x8");
                new CPUx86.Move("byte [_NATIVE_IDT_Contents + " + ((i*8) + 5) + "]",
                                "0x8E");
                new CPUx86.ShiftRight("eax",
                                      "eax",
                                      "16");
                new CPUx86.Move("[_NATIVE_IDT_Contents + " + ((i*8) + 6) + "]",
                                Registers.AL);
                new CPUx86.Move("[_NATIVE_IDT_Contents + " + ((i*8) + 7) + "]",
                                Registers.AH);
            }
            xFieldName = "_NATIVE_IDT_Pointer";
            xFieldData = "0x07FF,0,0"; //(_NATIVE_IDT_Contents and 0xFFFF),(_NATIVE_IDT_Contents shr 16)";
            aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>(aAssembler.CurrentGroup,
                                                                            new DataMember(xFieldName,
                                                                                           "dw",
                                                                                           xFieldData)));
            new CPUx86.Move("dword [_NATIVE_IDT_Pointer + 2]",
                            "_NATIVE_IDT_Contents");

            #endregion

            aAssembler.DataMembers.Add(new KeyValuePair<string, DataMember>("main",
                                                                            new DataMember("InterruptsEnabledFlag",
                                                                                           "dd",
                                                                                           "0")));

            new CPUx86.Move(Registers.EAX,
                            "_NATIVE_IDT_Pointer");
            new Label(".RegisterIDT");
            new CPUx86.Lidt(Registers.AtEAX);
            new CPUx86.Break();
            new CPUx86.Jump("__AFTER__ALL__ISR__HANDLER__STUBS__");
            var xInterruptsWithParam = new int[] {8, 10, 11, 12, 13, 14};
            for (int j = 0; j < 256; j++) {
                new Label("__ISR_Handler_" + j.ToString("X2"));
                //if (j < 0x20 || j > 0x2F || true) {
                new CPUx86.Cli();
                new CPUx86.Move("[InterruptsEnabledFlag]",
                                0);
                //}
                new CPUx86.Break();
                if (Array.IndexOf(xInterruptsWithParam,
                                  j) ==
                    -1) {
                    new CPUx86.Pushd("0");
                }
                new CPUx86.Pushd("0x" + j.ToString("X"));
                new CPUx86.Pushad();
                new CPUx86.Move("eax",
                                "0");
                new CPUx86.Move("ax",
                                "ds");
                new CPUx86.Push("eax");
                new CPUx86.Move("eax",
                                "0");
                new CPUx86.Move("ax",
                                "es");
                new CPUx86.Push("eax");
                new CPUx86.Move("eax",
                                "0");
                new CPUx86.Move("ax",
                                "fs");
                new CPUx86.Push("eax");
                new CPUx86.Move("eax",
                                "0");
                new CPUx86.Move("ax",
                                "gs");
                new CPUx86.Push("eax");
                new CPUx86.Move("ax",
                                "ss");
                new CPUx86.Push("eax");
                new CPUx86.Move("eax",
                                "esp");
                new CPUx86.Push("eax");
                new CPUx86.Move("eax",
                                "0x10");
                new CPUx86.Move("ds",
                                Registers.AX);
                new CPUx86.Move("es",
                                Registers.AX);
                new CPUx86.Move("fs",
                                Registers.AX);
                new CPUx86.Move("gs",
                                Registers.AX);
                new CPUx86.Move("ss",
                                Registers.AX);
                new CPUx86.Jump("0x8:__ISR_Handler_" + j.ToString("X2") + "_SetCS");
                new Label("__ISR_Handler_" + j.ToString("X2") + "_SetCS");
                MethodBase xHandler = GetInterruptHandler((byte) j);
                if (xHandler == null) {
                    xHandler = GetMethodDef(typeof(Cosmos.Hardware.PC.Interrupts).Assembly,
                                            typeof(Cosmos.Hardware.PC.Interrupts).FullName,
                                            "HandleInterrupt_Default",
                                            true);
                }
                new CPUx86.Call(Label.GenerateLabelName(xHandler));
                new CPUx86.Pop("eax");
                new CPUx86.Move("ss",
                                "ax");
                new CPUx86.Pop("eax");
                new CPUx86.Move("gs",
                                "ax");
                new CPUx86.Pop("eax");
                new CPUx86.Move("gs",
                                "ax");
                new CPUx86.Pop("eax");
                new CPUx86.Move("es",
                                "ax");
                new CPUx86.Pop("eax");
                new CPUx86.Move("ds",
                                "ax");
                new CPUx86.Popad();
                new CPUx86.Add("esp",
                               "8");
                new CPUx86.Break();
                new Label("__ISR_Handler_" + j.ToString("X2") + "_END");
                //if (j < 0x20 || j > 0x2F) {
                new CPUx86.Sti();
                new CPUx86.Move("[InterruptsEnabledFlag]",
                                1);
                //}
                new CPUx86.IRet();
            }
            new Label("__AFTER__ALL__ISR__HANDLER__STUBS__");
            new CPUx86.Noop();
            new CPUx86.Sti();
            new CPUx86.Move("[InterruptsEnabledFlag]",
                            1);
            // todo: add a datamember in which the interrupt state is stored
        }
    }
}