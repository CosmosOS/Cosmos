using System;
using System.Collections.Generic;
using System.Text;

namespace BreakpointsKernel
{
    public class MyTestClass
    {
        static MyTestClass()
        {
            Cosmos.Debug.Debugger.Send("In MyTestClass..cctor");
        }

        public static int Value = 43;
    }
}