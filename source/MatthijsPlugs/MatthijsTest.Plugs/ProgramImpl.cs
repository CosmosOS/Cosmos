using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace MatthijsTest.Plugs {
    [Plug(Target=typeof(Program))]
    public static class ProgramImpl {
        public static void DoTest() {
            Console.WriteLine("Alternative implementation");
        }
    }
}