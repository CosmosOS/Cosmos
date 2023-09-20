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

    public class Debugger
    {
        public static bool IgnoreAssert = false;
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

        #region Break
        public void Break() { }

        public static void DoBochsBreak() { }

        internal static void DoRealHalt() { }
        #endregion

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

        public unsafe void SendChannelCommand(byte aChannel, byte aCommand, byte[] aData)
        {
            fixed (byte* xPtr = &aData[0])
            {
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

        internal static void DoSend(string[] aStringArray)
        {
            for (int i = 0; i < aStringArray.Length; ++i)
            {
                DoSend(aStringArray[i]);
            }
        }

        #region Assert
        private static void SendKernelPanic(uint id) { }

        public static void SendCoreDump() { }

        #region Assert Function

        public static void DoAssert(int code)
        {
            DoAssert(true, code);
        }

        public static void DoAssert(bool condition)
        {
            DoAssert(condition, -1);
        }

        public static void DoAssert(bool condition, int code)
        {
            DoAssert(condition, code, false);
        }
        public static void DoAssert(bool condition, int code, string message)
        {
            DoAssert(condition, code, false, message);
        }
        public static void DoAssert(bool condition, bool DoBreak)
        {
            DoAssert(condition, -1, DoBreak, null);
        }
        public static void DoAssert(bool condition, bool DoBreak, string message)
        {
            DoAssert(condition, -1, DoBreak, message);
        }

        public static void DoAssert(bool condition, string message)
        {
            DoAssert(condition, -1, false, message);
        }

        public static void DoAssert(bool condition, int code, bool DoBreak)
        {
            DoAssert(condition, code, DoBreak, null);
        }

        public static void DoAssert(bool condition, int code, bool DoBreak, string message)
        {
            if (condition && !IgnoreAssert)
            {
                if (message != null) { DoSend(message); }
                if (DoBreak)
                {
                    DoBochsBreak();
                }
                if (code > -1)
                {
                    SendKernelPanic((uint)code);
                }
                else { SendCoreDump(); }
            }
        }

        [Conditional("COSMOSDEBUG")]
        public virtual void Assert(bool condition) => DoAssert(condition);

        [Conditional("COSMOSDEBUG")]
        public virtual void Assert(bool condition, int code) => DoAssert(condition, code);

        [Conditional("COSMOSDEBUG")]
        public virtual void Assert(bool condition, int code, string message) => DoAssert(condition, code, message);
        #endregion

        #region Fail Function

        public static void DoFail(uint code)
        {
            DoFail((int)code);
        }

        public static void DoFail(int code)
        {
            DoFail(code, null);
        }

        public static void DoFail(string message)
        {
            DoFail(-1, message);
        }

        public static void DoFail(int code, string message)
        {
            if (message != null) { DoSend(message); }
            if (code > -1)
            {
                SendKernelPanic((uint)code);
            }
            else { SendCoreDump(); } //  behave like assert function
            // halt
            while (true) { }
        }
        [Conditional("COSMOSDEBUG")]
        public virtual void Fail(string message) => DoFail(message);
        [Conditional("COSMOSDEBUG")]
        public virtual void Fail(int code) => DoFail(code);
        [Conditional("COSMOSDEBUG")]
        public virtual void Fail(int code, string message) => DoFail(code, message);
        #endregion

        #endregion

        #region Trace

        public void Send(string aText) => DoSend(aText);

        public void Send(string[] aStringArray) => DoSend(aStringArray);

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
        public virtual void SendInternal(float number) => DoSendNumber(number);

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

        public unsafe void SendMessageBox(int aLength, char* aText) { } // Plugged

        /// <summary>
        /// Displays a message box on connected debugging hosts.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public unsafe void SendMessageBox(string text)
        {
            // TODO: Need to fix this so it can send empty strings.
            // Sending empty strings locks it up right now
            if (text.Length == 0)
            {
                return;
            }

            var xChars = text.ToCharArray();
            fixed (char* xPtr = &xChars[0])
            {
                SendMessageBox(xChars.Length, xPtr);
            }
        }
        #endregion
        private int FromHex(string p)
        {
            p = p.ToLower();
            string hex = "0123456789abcdef";

            int ret = 0;

            for (int i = 0; i < p.Length; i++)
            {
                ret = ret * 16 + hex.IndexOf(p[i]);
            }
            return ret;
        }
    }
}