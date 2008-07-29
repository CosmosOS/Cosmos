using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    public static class BasicTest
    {
        public static void RunTest()
        {
            Console.WriteLine(" -- Testing Basic functionality --");

            Console.WriteLine("Byte 0: " + ((byte)0));
            Console.WriteLine("Byte 1: " + ((byte)1));
            Console.WriteLine("Byte 2: " + ((byte)2));
            Console.WriteLine("Byte 3: " + ((byte)3));
            Console.WriteLine("Byte 4: " + ((byte)4));
            Console.WriteLine("Byte 5: " + ((byte)5));
            Console.WriteLine("Byte 6: " + ((byte)6));

            Console.WriteLine("Integer 0: " + ((int)0));
            Console.WriteLine("Integer 6: " + ((int)6));
            Console.WriteLine("Integer 10: " + ((int)10));

            Console.WriteLine("Char F: " + ((char)'F'));
            Console.WriteLine("Char r: " + ((char)'r'));
            Console.WriteLine("Char o: " + ((char)'o'));
            Console.WriteLine("Char d: " + ((char)'d'));
            Console.WriteLine("Char e: " + ((char)'e'));

            byte[] bArray = new byte[4];
            bArray[0] = (byte)0;
            bArray[1] = (byte)1;
            bArray[2] = (byte)2;
            bArray[3] = (byte)3;

            Console.WriteLine("Byte array: " + bArray[0] + "." + bArray[1] + "." + bArray[2] + "." + bArray[3]);
        }
    }
}
