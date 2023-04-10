using System;
using System.Diagnostics;

namespace Cosmos.Debug.Kernel
{
    /// <summary>
    /// Provides a simplified interface for creating new instances of the
    /// <see cref="Debugger"/> class.
    /// </summary>
    public static class DebuggerFactory
    {
        /// <summary>
        /// Whether the created <see cref="Debugger"/> instances should output
        /// the given log inputs to the display console.
        /// </summary>
        public static bool WriteToConsole = false;

        /// <summary>
        /// Creates a new <see cref="Debugger"/> instance.
        /// </summary>
        /// <param name="ring">The virtual compile-time ring the debugger is operating in.</param>
        /// <param name="section">The section the debugger refers to.</param>
        public static Debugger CreateDebugger(string section = "")
        {
            if (WriteToConsole)
            {
                return new ConsoleDebugger(section);
            }
            else
            {
                return new Debugger(section);
            }
        }
    }

    /// <summary>
    /// Represents a categorized remote debugger, capable of communicating
    /// with an external host machine, including virtualizers.
    /// </summary>
    public class Debugger
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Debugger"/> class.
        /// </summary>
        /// <param name="ring">The virtual compile-time ring the debugger is operating in.</param>
        /// <param name="section">The section the debugger refers to.</param>
        public Debugger(string section)
        {
            Section = section;
        }

        /// <summary>
        /// The section the debugger refers to.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Triggers a software breakpoint.
        /// </summary>
        public void Break() { }

        /// <summary>
        /// Triggers a Bochs breakpoint.
        /// </summary>
        public static void DoBochsBreak() { }

        internal static void DoRealHalt() { }

        private static unsafe void ActualSend(int aLength, char* aText) { }

        /// <summary>
        /// Sends the pointer of the given object to any connected debugging hosts.
        /// </summary>
        public void SendPtr(object obj) { }

        /// <summary>
        /// Sends a 32-bit unsigned integer to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(uint number) { }

        /// <summary>
        /// Sends a 32-bit signed integer to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(int number) { }

        /// <summary>
        /// Sends a 64-bit unsigned integer to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(ulong number) { }

        /// <summary>
        /// Sends a 64-bit signed integer to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(long number) { }

        /// <summary>
        /// Sends a 32-bit floating-point number to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(float number) { }

        /// <summary>
        /// Sends a 64-bit floating-point number to connected debugging hosts.
        /// </summary>
        public static void DoSendNumber(double number) { }

        // NOTE: @ascpixi: There is no plug for this method anywhere. If we're
        //                 sure there is no end-user code using this method (I
        //                 doubt there is), then we can safely remove it. For
        //                 now, this is marked as [Obsolete].
        [Obsolete] internal static void DoSendCoreDump() { }

        /// <inheritdoc cref="DoSendNumber(uint)"/>
        public void SendNumber(uint number) => DoSendNumber(number);

        /// <inheritdoc cref="DoSendNumber(int)"/>
        public void SendNumber(int number) => DoSendNumber(number);

        /// <inheritdoc cref="DoSendNumber(ulong)"/>
        public void SendNumber(ulong number) => DoSendNumber(number);

        /// <inheritdoc cref="DoSendNumber(long)"/>
        public void SendNumber(long number) => DoSendNumber(number);

        /// <inheritdoc cref="DoSendNumber(float)"/>
        public void SendNumber(float number) => DoSendNumber(number);

        /// <inheritdoc cref="DoSendNumber(double)"/>
        public void SendNumber(double number) => DoSendNumber(number);
        
        public unsafe void SendChannelCommand(byte aChannel, byte aCommand, byte[] aData) {
            fixed (byte* xPtr = &aData[0]) {
                SendChannelCommand(aChannel, aCommand, aData.Length, xPtr);
            }
        }

        /// <summary>
        /// Sends a command and its associated data to the given debug channel.
        /// </summary>
        /// <param name="channel">The channel to send the data to.</param>
        /// <param name="command">The numeric command.</param>
        /// <param name="byteCount">The amount of bytes in the data associated with the command.</param>
        /// <param name="data">The data associated with the command</param>
        public static unsafe void SendChannelCommand(byte channel, byte command, int byteCount, byte* data) { }

        /// <summary>
        /// Sends a command to the given debug channel.
        /// </summary>
        /// <param name="channel">The channel to send the data to.</param>
        /// <param name="command">The numeric command.</param>
        public static void SendChannelCommand(byte channel, byte command) { }

        internal static void DoSend(string aText) { }

        internal static void DoSend(string[] aStringArray) {
            for (int i = 0; i < aStringArray.Length; ++i)
            {
                DoSend(aStringArray[i]);
            }
        }

        /// <summary>
        /// Sends a kernel panic error code to connected debugging hosts.
        /// </summary>
        public static void SendKernelPanic(uint id) { }

        /// <summary>
        /// Sends the given string to connected debugging hosts.
        /// </summary>
        /// <param name="text">The text/message to send.</param>
        public void Send(string text) => DoSend(text);

        /// <summary>
        /// Sends multiple strings to connected debugging hosts.
        /// </summary>
        /// <param name="stringArray">The strings to send.</param>
        public void Send(string[] stringArray) => DoSend(stringArray);

        /// <summary>
        /// Sends the given message to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(string text) => DoSend(text);

        /// <summary>
        /// Sends the given strings to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(string[] stringArray) => DoSend(stringArray);

        /// <summary>
        /// Sends the given 32-bit unsigned integer to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(uint number) => DoSendNumber(number);

        /// <summary>
        /// Sends the given 32-bit signed integer to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(int number) => DoSendNumber(number);

        /// <summary>
        /// Sends the given 64-bit unsigned integer to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(ulong number) => DoSendNumber(number);

        /// <summary>
        /// Sends the given 64-bit signed integer to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(long number) => DoSendNumber(number);

        /// <summary>
        /// Sends the given 32-bit floating-point number to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(float number)  => DoSendNumber(number);

        /// <summary>
        /// Sends the given 64-bit floating-point number to all connected debugging hosts.
        /// </summary>
        [Conditional("COSMOSDEBUG")]
        public virtual void SendInternal(double number) => DoSendNumber(number);

        //public void OldSend(string aText) {
        //    // TODO: Need to fix this so it can send empty strings.
        //    // Sending empty strings locks it up right now
        //    if (aText.Length == 0)
        //    {
        //        return;
        //    }

        //    var xChars = aText.ToCharArray();
        //    fixed (char* xPtr = &xChars[0])
        //    {
        //        ActualSend(xChars.Length, xPtr);
        //    }
        //}

        /// <summary>
        /// Displays a message box on connected debugging hosts.
        /// </summary>
        /// <param name="length">The length of the <paramref name="text"/> C-string.</param>
        /// <param name="text">The text to display in the message box, as a C-string.</param>
        public unsafe void SendMessageBox(int length, char* text) { } // Plugged

        /// <summary>
        /// Displays a message box on connected debugging hosts.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public unsafe void SendMessageBox(string text) {
            // TODO: Need to fix this so it can send empty strings.
            // Sending empty strings locks it up right now
            if (text.Length == 0) {
                return;
            }

            var xChars = text.ToCharArray();
            fixed (char* xPtr = &xChars[0]) {
                SendMessageBox(xChars.Length, xPtr);
            }
        }

        // TODO: Kudzu replacement methods for Cosmos.HAL.DebugUtil
        [Obsolete("This method is no longer used.")]
        public unsafe void SendMessage(string module, string data) {
            //string xSingleString;
            //xSingleString = "Message Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);

            DoSend("Message Module:");
            DoSend(module);
            DoSend("Data:");
            DoSend(data);
        }

        [Obsolete("This method is equivalent to a no-op in this version of Cosmos.")]
        public unsafe void SendError(string aModule, string aData) {
            //string xSingleString;
            //xSingleString = "Error Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);
        }

        [Obsolete("This method is equivalent to a no-op in this version of Cosmos.")]
        public unsafe void SendNumber(string aModule, string aDescription, uint aNumber, byte aBits) {
            //string xSingleString;
            //xSingleString = "Number Module: \"" + aModule + "\"";
            //xSingleString += " Description: \"" + aDescription + "\"";
            //xSingleString += " Number: \"" + CreateNumber(aNumber, aBits) + "\"";
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void WriteNumber(uint aNumber, byte aBits) {
            WriteNumber(aNumber, aBits, true);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void WriteNumber(uint aNumber, byte aBits, bool aWritePrefix) {
            Send(CreateNumber(aNumber, aBits, aWritePrefix));
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe string CreateNumber(uint aNumber, byte aBits) {
            return CreateNumber(aNumber, aBits, true);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe string CreateNumber(uint aNumber, byte aBits, bool aWritePrefix) {
            return "Cosmos.Debug.Debugger.CreateNumber(aNumber, aBits, aWritePrefix) not implemented";
            //string xNumberString = null;
            //uint xValue = aNumber;
            //byte xCurrentBits = aBits;
            //if (aWritePrefix)
            //{
            //    xNumberString += "0x";
            //}
            //while (xCurrentBits >= 4)
            //{
            //    xCurrentBits -= 4;
            //    byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
            //    string xDigitString = null;
            //    switch (xCurrentDigit)
            //    {
            //        case 0:
            //            xDigitString = "0";
            //            goto default;
            //        case 1:
            //            xDigitString = "1";
            //            goto default;
            //        case 2:
            //            xDigitString = "2";
            //            goto default;
            //        case 3:
            //            xDigitString = "3";
            //            goto default;
            //        case 4:
            //            xDigitString = "4";
            //            goto default;
            //        case 5:
            //            xDigitString = "5";
            //            goto default;
            //        case 6:
            //            xDigitString = "6";
            //            goto default;
            //        case 7:
            //            xDigitString = "7";
            //            goto default;
            //        case 8:
            //            xDigitString = "8";
            //            goto default;
            //        case 9:
            //            xDigitString = "9";
            //            goto default;
            //        case 10:
            //            xDigitString = "A";
            //            goto default;
            //        case 11:
            //            xDigitString = "B";
            //            goto default;
            //        case 12:
            //            xDigitString = "C";
            //            goto default;
            //        case 13:
            //            xDigitString = "D";
            //            goto default;
            //        case 14:
            //            xDigitString = "E";
            //            goto default;
            //        case 15:
            //            xDigitString = "F";
            //            goto default;
            //        default:
            //            xNumberString += xDigitString;
            //            break;
            //    }
            //}
            //return xNumberString;
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue) {
            WriteBinary(aModule, aMessage, aValue, 0, aValue.Length);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue, int aIndex, int aLength) {
            //string xSingleString;
            //xSingleString = "Binary Module = \"" + aModule + "\"";
            //xSingleString += " Message = " + aMessage + "\"";
            //xSingleString += " Value = \"";
            //for (int i = 0; i < aLength; i++)
            //{
            //    xSingleString += CreateNumber(aValue[aIndex + i], 8, false);
            //}
            //xSingleString += "\"";
            //Send(xSingleString);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void WriteBinary(string aModule, string aMessage, byte* aValue, int aIndex, int aLength) {
            //string xSingleString;
            //xSingleString = "Binary Module = \"" + aModule + "\"";
            //xSingleString += " Message = " + aMessage + "\"";
            //xSingleString += " Value = \"";
            //for (int i = 0; i < aLength; i++)
            //{
            //    xSingleString += CreateNumber(aValue[aIndex + i], 8, false);
            //}
            //xSingleString += "\"";
            //Send(xSingleString);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void ViewMemory() {
            ViewMemory(0);
        }

        [Obsolete("This method is currently not implemented.")]
        public unsafe void ViewMemory(int addr) {
            //while (true) {
            //    Console.Clear();
            //    Console.WriteLine();

            //    for (int j = 0; j < 20; j++) {
            //        int line = addr + j * 16;
            //        Console.Write(line.ToHex(8));
            //        Console.Write(": ");

            //        for (int i = 0; i < 16; i++) {
            //            if (i == 8) Console.Write("  ");
            //            Console.Write((*(byte*)(line + i)).ToHex(2) + " ");
            //        }
            //        Console.Write(" ");

            //        for (int i = 0; i < 16; i++) {
            //            byte b = (*(byte*)(line + i));
            //            if (i == 8) Console.Write(" ");
            //            if (b < 32 || b > 127)
            //                Console.Write(".");
            //            else
            //                Console.Write((char)b);
            //        }

            //        Console.WriteLine();
            //    }

            //    Console.WriteLine();

            //    Console.Write("Enter Hex Address (q to quit): ");
            //    string s = Console.ReadLine();
            //    if (s == "q")
            //        break;

            //    addr = FromHex(s);
            //}
        }


        [Obsolete("This method is currently not implemented.")]
        public void SendCoreDump() => DoSendCoreDump();

        private int FromHex(string p) {
            p = p.ToLower();
            string hex = "0123456789abcdef";

            int ret = 0;

            for (int i = 0; i < p.Length; i++) {
                ret = ret * 16 + hex.IndexOf(p[i]);
            }
            return ret;
        }
    }
}