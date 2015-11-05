using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StandAloneDebug
{
    class Debugger
    {
        public enum printtype
        {
            error,
            warrning,
            message
        }

        public static void Run()
        {
            while (Program.running)
            {
                if (Program.type == Program.types.Vmware)
                {
                    //execute vmware debugger     
                }
                else if (Program.type == Program.types.Broche)
                {
                    //execute broche debugger
                }      
            }
        }

        public static void Print(string message, printtype type)
        {
            if (type == printtype.error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
            }
            else if (type == printtype.warrning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
            }
            else if (type == printtype.message)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
            if (Program.printtofile == true)
            {
                if (type == printtype.error)
                {
                    Program.output += "[error] " + message + '\n';
                }
                else if (type == printtype.message)
                {
                    Program.output += "[message] " + message + '\n';
                }
                else if (type == printtype.warrning)
                {
                    Program.output += "[warrning] " + message + '\n';
                }
            }
        }
    }
}