using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Debug.Kernel
{
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
