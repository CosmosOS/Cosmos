using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StandAloneDebug
{
    class Debugger
    {
        public static void Run()
        {
            if (Program.type == Program.types.Vmware)
            {
                //execute vmware debugger
            }
            else if (Program.type == Program.types.Broche)
            {
                //execute broche debugger`
            }
        }

        static void Print(string message)
        {
            Console.WriteLine(message);
            if (Program.printtofile == true)
            {
                Program.output += message + '\n';
            }
        }
    }
}
