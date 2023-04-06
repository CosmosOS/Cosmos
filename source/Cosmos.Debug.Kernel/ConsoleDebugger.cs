using System;

namespace Cosmos.Debug.Kernel
{
    /// <summary>
    /// Represents a debugger that outputs all given inputs to the console in addition
    /// to its regular debugging host targets.
    /// </summary>
    class ConsoleDebugger : Debugger
    {
        public ConsoleDebugger(string aRing, string aSection) : base(aRing, aSection)
        {

        }

        void WriteText(string message)
        {
            Console.WriteLine($"[{Ring}][{Section}]: {message}");
        }

        public override void SendInternal(double aNumber)
        {
            WriteText(aNumber.ToString());
        }

        public override void SendInternal(float aNumber)
        {
            WriteText(aNumber.ToString());
        }

        public override void SendInternal(int aNumber)
        {
            WriteText(aNumber.ToString());
        }

        public override void SendInternal(long aNumber)
        {
            WriteText(aNumber.ToString());
        }

        public override void SendInternal(string aText)
        {
            WriteText(aText);
        }

        public override void SendInternal(string[] aStringArray)
        {
            for(int i = 0; i < aStringArray.Length; ++i)
            {
                WriteText(aStringArray[i]);
            }
        }
        public override void SendInternal(uint aNumber)
        {
            WriteText(aNumber.ToString());
        }

        public override void SendInternal(ulong aNumber)
        {
            WriteText(aNumber.ToString());
        }
    }
}
