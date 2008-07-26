using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware {
    // see http://osdev.berlios.de/v86.html
    public static class Virtual8086ModeMonitor {
        private static unsafe void HandleGPT(ref Interrupts.InterruptContext aContext,
                                             ref bool aHandled) {
            if (!aHandled) {
                if (CPU.IsVirtual8086Mode()) {
                    byte* xEIPPtr = (byte*)(aContext.EIP + (aContext.CS << 4)); // Do a address conversion
                    DebugUtil.WriteBinary("V86MM",
                                          "GPT from V86 Handled",
                                          xEIPPtr,
                                          0,
                                          8);
                    switch (xEIPPtr[0]) {
                        case 0xCD: {
                            // INT
                            byte xInterruptNumber = xEIPPtr[1];
                            Console.Write("Interrupt ");
                            Console.Write(xInterruptNumber.ToString());
                            Console.WriteLine(" called from V86 mode!");
                            switch (xInterruptNumber) {
                                case 5: {
                                    aContext.EFlags ^= (EFlagsEnum)0x20000;
                                    aContext.EIP = mOriginalEIP + 5;
                                    aContext.ESP = mOriginalESP;
                                    aContext.EBP = mOriginalEBP;
                                    //aContext.EBP = 0x30;
                                   // aContext.ESP -= 0x18; // magical number
                                    aContext.CS = 0x8;
                                    aHandled = true;
                                    DebugUtil.LogInterruptOccurred(ref aContext);
                                    CPU.CreateIDT(false);
                                    return;
                                }
                            }
                            aHandled = true;
                            aContext.EIP += 2;
                            return;
                        }
                    }
                }
            }
        }

        private static uint mOriginalEIP;
        private static uint mOriginalESP;
        private static uint mOriginalEBP;

        private static unsafe void HandleEnterVMMInterrupt(ref Interrupts.InterruptContext aContext) {
            if (aContext.EDX == 0) {
                // enter vmm
                /*
                 * EBX contains the start address of the 16bit code to execute
                 * ECX the count of bytes. The code is copied to 0x400
                 */
                if ((0x400 + aContext.ECX) >= 0xB8000) {
                    // how to exit here?
                    DebugUtil.SendError("VMM",
                                        "Canceled turning on, would overwrite text ui");
                    return;
                }
                byte* xCodeTarget = (byte*)0x400;
                byte* xCodeSource = (byte*)aContext.EBX;
                for (int i = 0; i < aContext.ECX; i++) {
                    xCodeTarget[i] = xCodeSource[i];
                }
                aContext.EFlags |= ((EFlagsEnum)0x20200);
                mOriginalEIP = aContext.EIP;
                mOriginalESP = aContext.ESP;
                mOriginalEBP = aContext.EBP;
                byte* xTSSPtr = CPU.GetTSS();
                Interrupts.TSS* xTSS = (Interrupts.TSS*)xTSSPtr;
                xTSS->EIP = mOriginalEIP;

                //xTSS->ESP = aContext.ESP;
                xTSS->ESP0 = aContext.ESP;
                //xTSS->ESP1 = aContext.ESP;
                //xTSS->ESP2 = aContext.ESP;
                xTSS->EBP = aContext.EBP;
                xTSS->EAX = aContext.EAX;
                xTSS->EBX = aContext.EBX;
                xTSS->ECX = aContext.ECX;
                xTSS->EDX = aContext.EDX;
                //xTSS->SS = 0x10;
                xTSS->SS0 = 0x10;
                //xTSS->SS1 = 0x10;
                //xTSS->SS2 = 0x10;
                xTSS->CS = 0x8;
                CPU.LoadTSS();
                aContext.CS = 1;
                aContext.EIP = 0x400 - 0x10;
                DebugUtil.LogInterruptOccurred(ref aContext);
                DebugUtil.SendMessage("VMM",
                                      "Turned on");
            }
        }

        public static unsafe void ExecuteTask(byte[] aCode) {
            uint xEAX = 0,
                 xEBX = 0,
                 xECX = 0,
                 xEDX = 0;
            fixed (byte* xPtr = &aCode[0]) {
                xEBX = (uint)xPtr;
            }
            xECX = (uint)aCode.Length;
            CPU.Interrupt30(ref xEAX,
                            ref xEBX,
                            ref xECX,
                            ref xEDX);
        }

        public static void Initialize() {
            Interrupts.GeneralProtectionFault += HandleGPT;
            Interrupts.Interrupt30 += HandleEnterVMMInterrupt;
        }
    }
}