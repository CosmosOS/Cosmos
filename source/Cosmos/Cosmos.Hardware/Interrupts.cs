using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Cosmos.Kernel;

namespace Cosmos.Hardware2 {
    public class Interrupts {
        [StructLayout(LayoutKind.Explicit, Size = 0x68)]
        public struct TSS
        {
            [FieldOffset(0)]
            public ushort Link;
            [FieldOffset(4)]
            public uint ESP0;
            [FieldOffset(8)]
            public ushort SS0;
            [FieldOffset(12)]
            public uint ESP1;
            [FieldOffset(16)]
            public ushort SS1;
            [FieldOffset(20)]
            public uint ESP2;
            [FieldOffset(24)]
            public ushort SS2;
            [FieldOffset(28)]
            public uint CR3;
            [FieldOffset(32)]
            public uint EIP;
            [FieldOffset(36)]
            public EFlagsEnum EFlags;
            [FieldOffset(40)]
            public uint EAX;
            [FieldOffset(44)]
            public uint ECX;
            [FieldOffset(48)]
            public uint EDX;
            [FieldOffset(52)]
            public uint EBX;
            [FieldOffset(56)]
            public uint ESP;
            [FieldOffset(60)]
            public uint EBP;
            [FieldOffset(64)]
            public uint ESI;
            [FieldOffset(68)]
            public uint EDI;
            [FieldOffset(72)]
            public ushort ES;
            [FieldOffset(76)]
            public ushort CS;
            [FieldOffset(80)]
            public ushort SS;
            [FieldOffset(84)]
            public ushort DS;
            [FieldOffset(88)]
            public ushort FS;
            [FieldOffset(92)]
            public ushort GS;
            [FieldOffset(96)]
            public ushort LDTR;
            [FieldOffset(102)]
            public ushort IOPBOffset;
        }

        [StructLayout(LayoutKind.Explicit, Size = 512)]
        public struct MMXContext
        {
        }
				
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct InterruptContext {
            [FieldOffset(0)]
            public unsafe MMXContext* MMXContext;

            [FieldOffset(4)]
            public uint EDI;
            
            [FieldOffset(8)]
            public uint ESI;

            [FieldOffset(12)]
            public uint EBP;

            [FieldOffset(16)]
            public uint ESP;

            [FieldOffset(20)]
            public uint EBX;

            [FieldOffset(24)]
            public uint EDX;

            [FieldOffset(28)]
            public uint ECX;

            [FieldOffset(32)]
            public uint EAX;

            [FieldOffset(36)]
            public uint Interrupt;

            [FieldOffset(40)]
            public uint Param;

            [FieldOffset(44)]
            public uint EIP;

            [FieldOffset(48)]
            public uint CS;

            [FieldOffset(52)]
            public EFlagsEnum EFlags;

            [FieldOffset(56)]
            public uint UserESP;
        }

        private static InterruptDelegate[] mIRQ_Handlers;

        public static void AddIRQHandler(byte IRQ, InterruptDelegate handler)
        {
            if (mIRQ_Handlers == null)
            {
                mIRQ_Handlers = new InterruptDelegate[256];
            }
            
            mIRQ_Handlers[IRQ] = handler;
        }

        private static void IRQ(uint irq,ref InterruptContext aContext)
        {
          var xCallback = mIRQ_Handlers[irq];
          if (xCallback != null) {
            xCallback(ref aContext);
          }            
        }

        public static void HandleInterrupt_Default(ref InterruptContext aContext) {
            //Console.Write("Interrupt ");
            //WriteNumber(aContext.Interrupt, 32);
            //Console.WriteLine("");
            DebugUtil.LogInterruptOccurred(ref aContext);
            if (aContext.Interrupt >= 0x20 && aContext.Interrupt <= 0x2F) {
                if (aContext.Interrupt >= 0x28) {
                    PIC.SignalSecondary();
                } else {
                    PIC.SignalPrimary();
                }
            }
        }

        public delegate void InterruptDelegate(ref InterruptContext aContext);

        public delegate void ExceptionInterruptDelegate(ref InterruptContext aContext,
                                                        ref bool aHandled);

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
        public static void HandleInterrupt_20(ref InterruptContext aContext) {
            PIT.HandleInterrupt();
            PIC.SignalPrimary();
        }

        //public static InterruptDelegate IRQ01;
        //IRQ 1 - Keyboard. Reserved for the system. Cannot be altered even if no keyboard is present or needed.
        public static void HandleInterrupt_21(ref InterruptContext aContext) {
            ////Change area
            ////
            //// Triggers IL2CPU error
          //DebugUtil.LogInterruptOccurred(ref aContext);
          IRQ(1, ref aContext);
            ////mIRQ_Handlers[1](ref aContext);
            ////
            //// Old keyboard
            ////Cosmos.Hardware2.Keyboard.HandleKeyboardInterrupt();
            ////
            //// New Keyboard
            ////Cosmos.Hardware2.PC
            ////
            //// - End change area
            //Console.WriteLine("Signal PIC primary");
            PIC.SignalPrimary();
        }

        //IRQ 5 - (Added for ES1370 AudioPCI)
        //public static InterruptDelegate IRQ05;

        public static void HandleInterrupt_25(ref InterruptContext aContext) {
            IRQ(5,ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 09 - (Added for AMD PCNet network card)
        //public static InterruptDelegate IRQ09;

        public static void HandleInterrupt_29(ref InterruptContext aContext) {
            IRQ(9,ref aContext);
            PIC.SignalSecondary();
        }

        //IRQ 10 - (Added for VIA Rhine network card)
        //public static InterruptDelegate IRQ10;

        public static void HandleInterrupt_2A(ref InterruptContext aContext)
        {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //Console.WriteLine("IRQ 11 raised!");

            IRQ(10,ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 11 - (Added for RTL8139 network card)
        //public static InterruptDelegate IRQ11;

        public static void HandleInterrupt_2B(ref InterruptContext aContext) {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //Console.WriteLine("IRQ 11 raised!");

            IRQ(11,ref aContext);

            PIC.SignalSecondary();
        }

        public static void HandleInterrupt_2C(ref InterruptContext aContext)
        {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //A> Vermeulen
            //Commented out below
            //Console.WriteLine("IRQ 12 raised!");

            IRQ(12, ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 14 - Primary IDE. If no Primary IDE this can be changed
        public static void HandleInterrupt_2E(ref InterruptContext aContext) {
            Cosmos.Debug.Debugger.SendMessage("IRQ",
                                                  "Primary IDE");
            //Storage.ATAOld.HandleInterruptPrimary();
            Storage.ATA.ATA.HandleInterruptPrimary();
            PIC.SignalSecondary();
        }

        public static event InterruptDelegate Interrupt30;
        // Interrupt 0x30, enter VMM
        public static void HandleInterrupt_30(ref InterruptContext aContext) {
            DebugUtil.LogInterruptOccurred(ref aContext);
            if (Interrupt30 != null) {
                Interrupt30(ref aContext);
            } else {
                DebugUtil.SendError("Interrupts",
                                    "Interrupt 0x30 not handled!");
            }
        }

        public static void HandleInterrupt_35(ref InterruptContext aContext) {
            Cosmos.Debug.Debugger.SendMessage("Interrupts",
                                                  "Interrupt 35 handler");
            DebugUtil.LogInterruptOccurred(ref aContext);
            Console.WriteLine("Interrupt 0x35 occurred");
            aContext.EAX *= 2;
            aContext.EBX *= 2;
            aContext.ECX *= 2;
            aContext.EDX *= 2;
        }

        //IRQ 15 - Secondary IDE
        public static void HandleInterrupt_2F(ref InterruptContext aContext) {
            Storage.ATA.ATA.HandleInterruptSecondary();
            Cosmos.Debug.Debugger.SendMessage("IRQ",
                                                  "Secondary IDE");
            PIC.SignalSecondary();
        }

        public static void HandleInterrupt_00(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Divide by zero",
                            "EDivideByZero",
                            ref aContext);
        }

        public static void HandleInterrupt_06(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Invalid Opcode",
                            "EInvalidOpcode",
                            ref aContext);
        }

        public static event ExceptionInterruptDelegate GeneralProtectionFault;

        public static void HandleInterrupt_0D(ref InterruptContext aContext) {
            bool xHandled = false;
            DebugUtil.LogInterruptOccurred(ref aContext);
            if (GeneralProtectionFault != null) {
                GeneralProtectionFault(ref aContext,
                                       ref xHandled);
            }
            if (!xHandled) {
                HandleException(aContext.EIP,
                                "General Protection Fault",
                                "GPF",
                                ref aContext);
            }
        }

        public static void HandleInterrupt_01(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Debug Exception",
                            "Debug Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_02(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Non Maskable Interrupt Exception",
                            "Non Maskable Interrupt Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_03(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Breakpoint Exception",
                            "Breakpoint Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_04(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Into Detected Overflow Exception",
                            "Into Detected Overflow Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_05(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Out of Bounds Exception",
                            "Out of Bounds Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_07(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "No Coprocessor Exception",
                            "No Coprocessor Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_08(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Double Fault Exception",
                            "Double Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_09(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Coprocessor Segment Overrun Exception",
                            "Coprocessor Segment Overrun Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0A(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Bad TSS Exception",
                            "Bad TSS Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0B(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Segment Not Present",
                            "Segment Not Present",
                            ref aContext);
        }

        public static void HandleInterrupt_0C(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Stack Fault Exception",
                            "Stack Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0E(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Page Fault Exception",
                            "Page Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0F(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Unknown Interrupt Exception",
                            "Unknown Interrupt Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_10(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Coprocessor Fault Exception",
                            "Coprocessor Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_11(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Alignment Exception",
                            "Alignment Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_12(ref InterruptContext aContext) {
            HandleException(aContext.EIP,
                            "Machine Check Exception",
                            "Machine Check Exception",
                            ref aContext);
        }

        private static void HandleException(uint aEIP,
                                            string aDescription,
                                            string aName,
                                            ref InterruptContext ctx) {
            const string SysFault = "*** System Fault ***  ";

            //Console.ForegroundColor = ConsoleColor.White;
            //Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(SysFault);
            //for (int i = 0; i < Console.WindowWidth - SysFault.Length; i++)
            //    Console.Write(" ");

            //Console.BackgroundColor = ConsoleColor.Black;

            Console.Write(aDescription);
            Console.Write(" at ");
            WriteNumber(aEIP,
                        32);

            Console.WriteLine();
//            Console.WriteLine("Register States:");
            // TODO: Register states

            Cosmos.Debug.Debugger.SendMessage("Exceptions",
                                                  aName);
            Console.WriteLine();
            while (true) {
                ;
            }
        }

        // This is to trick IL2CPU to compile it in
        //TODO: Make a new attribute that IL2CPU sees when scanning to force inclusion so we dont have to do this.
        public static void Init()
        {
            #region Compiler magic
            bool xTest = false;
            if (xTest) {
                unsafe {
                    InterruptContext xCtx = new InterruptContext();
                    HandleInterrupt_Default(ref xCtx);
                    HandleInterrupt_00(ref xCtx);
                    HandleInterrupt_01(ref xCtx);
                    HandleInterrupt_02(ref xCtx);
                    HandleInterrupt_03(ref xCtx);
                    HandleInterrupt_04(ref xCtx);
                    HandleInterrupt_05(ref xCtx);
                    HandleInterrupt_06(ref xCtx);
                    HandleInterrupt_07(ref xCtx);
                    HandleInterrupt_08(ref xCtx);
                    HandleInterrupt_09(ref xCtx);
                    HandleInterrupt_0A(ref xCtx);
                    HandleInterrupt_0B(ref xCtx);
                    HandleInterrupt_0C(ref xCtx);
                    HandleInterrupt_0D(ref xCtx);
                    HandleInterrupt_0E(ref xCtx);
                    HandleInterrupt_0F(ref xCtx);
                    HandleInterrupt_10(ref xCtx);
                    HandleInterrupt_11(ref xCtx);
                    HandleInterrupt_12(ref xCtx);
                    HandleInterrupt_20(ref xCtx);
                    HandleInterrupt_21(ref xCtx);
                    HandleInterrupt_25(ref xCtx);
                    HandleInterrupt_29(ref xCtx);
                    HandleInterrupt_2A(ref xCtx);
                    HandleInterrupt_2B(ref xCtx);
                    HandleInterrupt_2C(ref xCtx);
                    HandleInterrupt_2E(ref xCtx);
                    HandleInterrupt_2F(ref xCtx);
                    HandleInterrupt_30(ref xCtx);
                    HandleInterrupt_35(ref xCtx);
                }
            }
            #endregion
        }

        public static void WriteNumber(uint aValue,
                                        byte aBitCount) {
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

        public static void WriteNumberWithoutPrefix(uint aValue,
                                byte aBitCount)
        {
            uint xValue = aValue;
            byte xCurrentBits = aBitCount;
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
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