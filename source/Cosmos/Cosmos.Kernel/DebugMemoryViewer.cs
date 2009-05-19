using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel
{
    public class DebugMemoryViewer
    {
        public unsafe static void View()
        {
            View(0);
        }

        public unsafe static void View(int addr)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();

                for (int j = 0; j < 20; j++)
                {
                    int line = addr + j * 16;
                    Console.Write(line.ToHex(8));
                    Console.Write(": ");

                    for (int i = 0; i < 16; i++)
                    {
                        if (i == 8) Console.Write("  ");
                        Console.Write((*(byte*)(line + i)).ToHex(2) + " ");
                    }
                    Console.Write(" ");

                    for (int i = 0; i < 16; i++)
                    {
                        byte b = (*(byte*)(line + i));
                        if (i == 8) Console.Write(" ");
                        if (b < 32 || b > 127)
                            Console.Write(".");
                        else
                            Console.Write((char)b);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();

                Console.Write("Enter Hex Address (q to quit): ");
                string s = Console.ReadLine();
                if (s == "q")
                    break;

                addr = FromHex(s);
            }
        }

        private static int FromHex(string p)
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