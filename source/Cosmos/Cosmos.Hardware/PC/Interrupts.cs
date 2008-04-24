using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Hardware.PC {
    public class Interrupts {
        [StructLayout(LayoutKind.Explicit, Size=76)]
        public struct InterruptContext {
			[FieldOffset(0)]
			public uint SS;
			[FieldOffset(4)]
			public uint GS;
			[FieldOffset(8)]
			public uint FS;
			[FieldOffset(12)]
			public uint ES;
			[FieldOffset(16)]
			public uint DS;
			[FieldOffset(20)]
			public uint EDI;
			[FieldOffset(24)]
			public uint ESI;
			[FieldOffset(28)]
			public uint EBP;
			[FieldOffset(32)]
			public uint ESP;
			[FieldOffset(36)]
			public uint EBX;
			[FieldOffset(40)]
			public uint EDX;
			[FieldOffset(44)]
			public uint ECX;
			[FieldOffset(48)]
			public uint EAX;
			[FieldOffset(52)]
			public uint Interrupt;
			[FieldOffset(56)]
			public uint Param;
			[FieldOffset(60)]
			public uint EIP;
			[FieldOffset(64)]
			public uint CS;
			[FieldOffset(68)]
			public uint EFlags;
			[FieldOffset(72)]
			public uint UserESP;
        }

        public unsafe static void HandleInterrupt_Default(InterruptContext* aContext) {
			//Console.Write("Interrupt ");
			//WriteNumber(aContext->Interrupt, 32);
			//Console.WriteLine("");
			DebugUtil.LogInterruptOccurred(aContext);
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

        //IRQ 11 - (Added for RTL8139 network card)
        static public InterruptDelegate IRQ11;
        public static unsafe void HandleInterrupt_2B(InterruptContext* aContext)
        {
            //Debugging....
            DebugUtil.LogInterruptOccurred(aContext);
            Cosmos.Hardware.DebugUtil.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            Console.WriteLine("IRQ 11 raised!");

            if (IRQ11 != null)
                IRQ11();

            Bus.CPU.PIC.SignalSecondary();
        }

        //IRQ 14 - Primary IDE. If no Primary IDE this can be changed
        public static unsafe void HandleInterrupt_2E(InterruptContext* aContext) {
			Cosmos.Hardware.DebugUtil.SendMessage("IRQ", "Primary IDE");
			//Storage.ATAOld.HandleInterruptPrimary();
			Storage.ATA2.ATA.HandleInterruptPrimary();
            Bus.CPU.PIC.SignalSecondary();
        }

		public static unsafe void HandleInterrupt_35(InterruptContext* aContext) {
			Cosmos.Hardware.DebugUtil.SendMessage("Interrupts", "Interrupt 35 handler");
			Cosmos.Hardware.DebugUtil.SendNumber("Interrupts", "Context address", (uint)aContext, 32);
			DebugUtil.LogInterruptOccurred(aContext);
			Console.WriteLine("Halting");
			while (true)
				;
		}

        //IRQ 15 - Secondary IDE
        public static unsafe void HandleInterrupt_2F(InterruptContext* aContext) {
			Storage.ATA2.ATA.HandleInterruptSecondary();
			Cosmos.Hardware.DebugUtil.SendMessage("IRQ", "Secondary IDE");
            Bus.CPU.PIC.SignalSecondary();
        }

        public static unsafe void HandleInterrupt_00(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Divide by zero", "EDivideByZero", aContext);
        }

        public static unsafe void HandleInterrupt_06(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Invalid Opcode", "EInvalidOpcode", aContext);
        }

        public static unsafe void HandleInterrupt_0D(InterruptContext* aContext) {
			HandleException(aContext->EIP, "General Protection Fault", "GPF", aContext);
        }

		public static unsafe void HandleInterrupt_01(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Debug Exception", "Debug Exception", aContext);
		}
		public static unsafe void HandleInterrupt_02(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Non Maskable Interrupt Exception", "Non Maskable Interrupt Exception", aContext);
		}
		public static unsafe void HandleInterrupt_03(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Breakpoint Exception", "Breakpoint Exception", aContext);
		}
		public static unsafe void HandleInterrupt_04(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Into Detected Overflow Exception", "Into Detected Overflow Exception", aContext);
		}
		public static unsafe void HandleInterrupt_05(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Out of Bounds Exception", "Out of Bounds Exception", aContext);
		}
		public static unsafe void HandleInterrupt_07(InterruptContext* aContext) {
			HandleException(aContext->EIP, "No Coprocessor Exception", "No Coprocessor Exception", aContext);
		}
		public static unsafe void HandleInterrupt_08(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Double Fault Exception", "Double Fault Exception", aContext);
		}
		public static unsafe void HandleInterrupt_09(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Coprocessor Segment Overrun Exception", "Coprocessor Segment Overrun Exception", aContext);
		}
		public static unsafe void HandleInterrupt_0A(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Bad TSS Exception", "Bad TSS Exception", aContext);
		}
		public static unsafe void HandleInterrupt_0B(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Segment Not Present", "Segment Not Present", aContext);
		}
		public static unsafe void HandleInterrupt_0C(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Stack Fault Exception", "Stack Fault Exception", aContext);
		}
		public static unsafe void HandleInterrupt_0E(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Page Fault Exception", "Page Fault Exception", aContext);
		}
		public static unsafe void HandleInterrupt_0F(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Unknown Interrupt Exception", "Unknown Interrupt Exception", aContext);
		}
		public static unsafe void HandleInterrupt_10(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Coprocessor Fault Exception", "Coprocessor Fault Exception", aContext);
		}
		public static unsafe void HandleInterrupt_11(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Alignment Exception", "Alignment Exception", aContext);
		}
		public static unsafe void HandleInterrupt_12(InterruptContext* aContext) {
			HandleException(aContext->EIP, "Machine Check Exception", "Machine Check Exception", aContext);
		}

        private static unsafe void HandleException(uint aEIP, string aDescription, string aName, InterruptContext* ctx) {
            const string SysFault = "System Fault";

            //Console.ForegroundColor = ConsoleColor.White;
            //Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(SysFault);
			//for (int i = 0; i < Console.WindowWidth - SysFault.Length; i++)
			//    Console.Write(" ");

            //Console.BackgroundColor = ConsoleColor.Black;

            Console.Write(aDescription);
            Console.Write(" at ");
            WriteNumber(aEIP, 32);

            Console.WriteLine();
//            Console.WriteLine("Register States:");
            // TODO: Register states

            Cosmos.Hardware.DebugUtil.SendMessage("Exceptions", aName);
            Console.WriteLine();
            while (true)
                ;
        }

        // This is to trick IL2CPU to compile it in
        //TODO: Make a new attribute that IL2CPU sees when scanning to force inclusion so we dont have to do this.
        public static void Init() {
            bool xTest = false;
            if (xTest) {
                unsafe {
                    HandleInterrupt_Default(null);
					HandleInterrupt_00(null);
					HandleInterrupt_01(null);
					HandleInterrupt_02(null);
					HandleInterrupt_03(null);
					HandleInterrupt_04(null);
					HandleInterrupt_05(null);
					HandleInterrupt_06(null);
					HandleInterrupt_07(null);
					HandleInterrupt_08(null);
					HandleInterrupt_09(null);
					HandleInterrupt_0A(null);
					HandleInterrupt_0B(null);
					HandleInterrupt_0C(null);
					HandleInterrupt_0D(null);
					HandleInterrupt_0E(null);
					HandleInterrupt_0F(null);
					HandleInterrupt_10(null);
					HandleInterrupt_11(null);
					HandleInterrupt_12(null);
                    HandleInterrupt_20(null);
                    HandleInterrupt_21(null);
                    HandleInterrupt_2B(null);
                    HandleInterrupt_2E(null);
                    HandleInterrupt_2F(null);
					HandleInterrupt_35(null);
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
