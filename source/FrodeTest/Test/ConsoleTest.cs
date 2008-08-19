using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class ConsoleTest
    {
        public static void RunTest()
        {
            char a = 'C';
            Console.Write(a);

            byte b = 1;
            Console.Write(b); //converted to int

            bool c = true;
            Console.Write(c);

            char[] d = new char[5] { 'a', 'b', 'c', 'd', 'e' };
            Console.Write(d, 2, 3);
            Console.Write(d);

            Console.WriteLine();
            Console.WriteLine(a); //char
            Console.WriteLine(b); //byte, int
            Console.WriteLine(c); //bool
            Console.WriteLine(d); //char[]
            Console.WriteLine(d, 2, 3); //char[] overload
        }
    }
}
