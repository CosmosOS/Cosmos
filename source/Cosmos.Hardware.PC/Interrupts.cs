using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Hardware.PC {
    public class Interrupts {
        [StructLayout(LayoutKind.Sequential)]
        public struct InterruptContext {
            public uint GS;
            public uint FS;
            public uint ES;
            public uint DS;
            public uint EDI;
            public uint ESI;
            public uint EBP;
            /// <summary>
            /// Doesn't work yet
            /// </summary>
            public uint ESP;
            public uint EBX;
            public uint EDX;
            public uint ECX;
            public uint EAX;
            public uint Interrupt;
            public uint Param;
            public uint EIP;
            public uint CS;
            public uint EFlags;
            /// <summary>
            /// Doesn't work yet
            /// </summary>
            public uint UserESP;
            /// <summary>
            /// Doesn't work yet
            /// </summary>
            public uint SS;
        }

        public unsafe static void HandleInterrupt_Default(InterruptContext* aContext) {
            Console.Write("Interrupt ");
            WriteNumber(aContext->Interrupt, 32);
            Console.WriteLine("");
            //DebugUtil.LogInterruptOccurred(aContext);
            if (aContext->Interrupt >= 0x20 && aContext->Interrupt <= 0x2F) {
                if (aContext->Interrupt >= 0x28) {
                    Bus.CPU.PIC.SignalSecondary();
                } else {
                    Bus.CPU.PIC.SignalPrimary();
                }
            }
        }

        public delegate void InterruptDelegate();

        //IRQ 2 - Cascaded signals from IRQs 8-15. A device configured to use IRQ 2 will actually be using IRQ 9
        //IRQ 3 - COM2 (Default) and COM4 (User) serial ports
        //IRQ 4 - COM1 (Default) and COM3 (User) serial ports
        //IRQ 5 - LPT2 Parallel Port 2 or sound card
        //IRQ 6 - Floppy disk controller
        //IRQ 7 - LPT1 Parallel Port 1 or sound card (8-bit Sound Blaster and compatibles)

        //IRQ 8 - Real time clock
        //IRQ 9 - Free / Open interrupt / Available / SCSI. Any devices configured to use IRQ 2 will actually be using IRQ 9.
        //IRQ 10 - Free / Open interrupt / Available / SCSI
        //IRQ 11 - Free / Open interrupt / Available / SCSI
        //IRQ 12 - PS/2 connector Mouse. If no PS/2 connector mouse is used, this can be used for other peripherals
        //IRQ 13 - ISA / Math Co-Processor

        //IRQ 0 - System timer. Reserved for the system. Cannot be changed by a user.
        public static unsafe void HandleInterrupt_20(InterruptContext* aContext) {
            PIT.HandleInterrupt();
            Bus.CPU.PIC.SignalPrimary();
        }

        static public InterruptDelegate IRQ01;
        //IRQ 1 - Keyboard. Reserved for the system. Cannot be altered even if no keyboard is present or needed.
        public static unsafe void HandleInterrupt_21(InterruptContext* aContext) {
            //Change area
            //
            // Triggers IL2CPU error
            //IRQ01(); 
            //
            // Old keyboard
            Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt();
            //
            // New Keyboard
            //Cosmos.Hardware.PC
            //
            // - End change area

            Bus.CPU.PIC.SignalPrimary();
        }

        //IRQ 14 - Primary IDE. If no Primary IDE this can be changed
        public static unsafe void HandleInterrupt_2E(InterruptContext* aContext) {
            Storage.ATAOld.HandleInterruptPrimary();
            Bus.CPU.PIC.SignalSecondary();
        }

        //IRQ 15 - Secondary IDE
        public static unsafe void HandleInterrupt_2F(InterruptContext* aContext) {
            Storage.ATAOld.HandleInterruptSecondary();
            Bus.CPU.PIC.SignalSecondary();
        }

        public static unsafe void HandleInterrupt_00(InterruptContext* aContext) {
            HandleException(aContext->EIP, "Divide by zero", "EDivideByZero", aContext->EBP);
        }

        public static unsafe void HandleInterrupt_06(InterruptContext* aContext) {
            HandleException(aContext->EIP, "Invalid Opcode", "EInvalidOpcode", aContext->EBP);
        }

        public static unsafe void HandleInterrupt_0D(InterruptContext* aContext) {
            HandleException(aContext->EIP, "General Protection Fault", "GPF", aContext->EBP);
        }

        private static void HandleException(uint aEIP, string aDescription, string aName, uint aEBP) {
            Console.Write(aDescription);
            Console.Write(" at ");
            WriteNumber(aEIP, 32);
            Console.Write(" (EBP = ");
            Console.Write(aEBP.ToString());
            DebugUtil.SendMessage("Exceptions", aName);
            Console.WriteLine();
            Console.WriteLine("--System Halted!");
            while (true)
                ;
        }

        // This is to trick IL2CPU to compile it in
        //TODO: Make a new attribute that IL2CPU sees when scanning to force inclusion so we dont have to do this.
        public static void IncludeAllHandlers() {
            bool xTest = false;
            if (xTest) {
                unsafe {
                    HandleInterrupt_Default(null);
                    HandleInterrupt_00(null);
                    HandleInterrupt_06(null);
                    HandleInterrupt_0D(null);
                    HandleInterrupt_20(null);
                    HandleInterrupt_21(null);
                    HandleInterrupt_2E(null);
                    HandleInterrupt_2F(null);
                }
            }
        }

        private static void WriteNumber(uint aValue, byte aBitCount) {
            uint xValue = aValue;
            byte xCurrentBits = aBitCount;
            Console.Write("0x");
            while (xCurrentBits >= 4) {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit) {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        Console.Write(xDigitString);
                        break;
                }
            }
        }

    }
}
