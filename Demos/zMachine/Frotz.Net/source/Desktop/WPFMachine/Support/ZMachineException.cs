using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMachine
{
    class ZMachineException : Exception
    {
        public ZMachineException(String Message) : base(Message) { }
    }
}
